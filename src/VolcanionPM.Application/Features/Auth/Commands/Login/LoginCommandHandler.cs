using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Auth;

namespace VolcanionPM.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository, 
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ILogger<LoginCommandHandler> logger,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        // Check if account is locked
        if (user.IsLockedOut)
        {
            var lockoutEndMinutes = (user.LockoutEndDate!.Value - DateTime.UtcNow).TotalMinutes;
            _logger.LogWarning("Login failed: Account locked for user {UserId}. Lockout ends in {Minutes} minutes", 
                user.Id, (int)lockoutEndMinutes);
            return Result<AuthResponseDto>.Failure(
                $"Account is locked due to multiple failed login attempts. Try again in {(int)lockoutEndMinutes + 1} minutes.");
        }

        // Verify password using BCrypt
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
            
            // Record failed login attempt
            var maxAttempts = _configuration.GetValue<int>("Security:AccountLockout:MaxFailedAttempts", 5);
            var lockoutDuration = _configuration.GetValue<int>("Security:AccountLockout:LockoutDurationMinutes", 15);
            user.RecordFailedLogin(maxAttempts, lockoutDuration);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            var remainingAttempts = maxAttempts - user.FailedLoginAttempts;
            if (remainingAttempts > 0)
            {
                return Result<AuthResponseDto>.Failure(
                    $"Invalid email or password. {remainingAttempts} attempt(s) remaining before account lockout.");
            }
            
            return Result<AuthResponseDto>.Failure(
                $"Account has been locked due to too many failed login attempts. Try again in {lockoutDuration} minutes.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: Inactive account for user {UserId}", user.Id);
            return Result<AuthResponseDto>.Failure("User account is inactive");
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update user with refresh token and last login (this also resets failed login attempts)
        user.UpdateRefreshToken(refreshToken, refreshTokenExpiry, "System");
        user.RecordLogin();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged in successfully: {UserId} ({Email})", user.Id, user.Email.Value);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            }
        };

        return Result<AuthResponseDto>.Success(response);
    }
}
