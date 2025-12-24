namespace VolcanionPM.Application.Common.Interfaces;

/// <summary>
/// Email service interface for sending various types of emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a password reset email with token
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email confirmation email
    /// </summary>
    Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email to new users
    /// </summary>
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a generic email
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
