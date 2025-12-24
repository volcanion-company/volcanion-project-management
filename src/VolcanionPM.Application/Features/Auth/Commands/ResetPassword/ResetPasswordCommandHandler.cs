using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing password reset for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password reset failed: User not found for email {Email}", request.Email);
            return Result<bool>.Failure("Invalid reset token or email");
        }

        // Validate the reset token
        if (!user.IsPasswordResetTokenValid(request.ResetToken))
        {
            _logger.LogWarning("Password reset failed: Invalid or expired token for user {UserId}", user.Id);
            return Result<bool>.Failure("Invalid or expired reset token");
        }

        // Hash the new password
        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

        // Update password and clear reset token
        user.ChangePassword(passwordHash, "System");
        user.ClearPasswordResetToken("System");
        user.RevokeRefreshToken("System"); // Revoke all refresh tokens for security

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);

        return Result<bool>.Success(true);
    }
}
