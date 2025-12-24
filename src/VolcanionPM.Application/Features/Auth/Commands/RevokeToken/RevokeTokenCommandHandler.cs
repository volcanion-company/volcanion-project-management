using MediatR;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;

    public RevokeTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user == null)
        {
            return Result<bool>.Failure("Invalid refresh token");
        }

        // Revoke the refresh token
        user.RevokeRefreshToken("System");

        return Result<bool>.Success(true);
    }
}
