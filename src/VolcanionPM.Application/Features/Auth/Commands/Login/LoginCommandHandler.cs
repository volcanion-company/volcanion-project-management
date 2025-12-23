using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Auth;

namespace VolcanionPM.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository, 
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        // Verify password using BCrypt
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("User account is inactive");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

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
