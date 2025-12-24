using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user == null)
        {
            return Result<RefreshTokenResponse>.Failure("Invalid refresh token");
        }

        // Validate refresh token
        if (user.RefreshTokenExpiryDate == null || user.RefreshTokenExpiryDate <= DateTime.UtcNow)
        {
            return Result<RefreshTokenResponse>.Failure("Refresh token has expired");
        }

        if (!user.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure("User account is deactivated");
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update user with new refresh token
        user.UpdateRefreshToken(newRefreshToken, newRefreshTokenExpiry, "System");
        user.RecordLogin();

        var response = new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = newRefreshTokenExpiry
        };

        return Result<RefreshTokenResponse>.Success(response);
    }
}
