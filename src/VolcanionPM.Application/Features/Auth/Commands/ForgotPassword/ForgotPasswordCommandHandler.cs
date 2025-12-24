using MediatR;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration attacks
        if (user == null)
        {
            _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
            return Result<bool>.Success(true);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Forgot password requested for inactive user: {UserId}", user.Id);
            return Result<bool>.Success(true);
        }

        // Generate a secure random token
        var resetToken = GenerateSecureToken();
        var expiryDate = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

        // Store the token in the database
        user.SetPasswordResetToken(resetToken, expiryDate, "System");
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send password reset email
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                user.GetFullName(),
                resetToken,
                cancellationToken);

            _logger.LogInformation("Password reset email sent successfully to {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
            return Result<bool>.Failure("Failed to send password reset email. Please try again later.");
        }

        return Result<bool>.Success(true);
    }

    private string GenerateSecureToken()
    {
        // Generate a 6-digit numeric token
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
