using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Auth;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure("Email is already registered");
        }

        // Verify organization exists
        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization == null)
        {
            return Result<AuthResponseDto>.Failure($"Organization with ID {request.OrganizationId} not found");
        }

        // Create email value object
        var email = request.Email;

        // Hash password using BCrypt
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            request.FirstName,
            request.LastName,
            email,
            passwordHash,
            request.OrganizationId,
            UserRole.Developer, // Default role
            null, // phoneNumber
            "System"
        );

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
