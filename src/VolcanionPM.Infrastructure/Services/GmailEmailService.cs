using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.Infrastructure.Services;

/// <summary>
/// Gmail SMTP email service implementation
/// </summary>
public class GmailEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public GmailEmailService(IConfiguration configuration, ILogger<GmailEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Load email configuration
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@volcanionpm.com";
        _fromName = _configuration["Email:FromName"] ?? "Volcanion PM";
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password - Volcanion PM";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Password Reset Request</h2>
                <p>Hello {userName},</p>
                <p>We received a request to reset your password for your Volcanion PM account.</p>
                <p>Please use the following token to reset your password:</p>
                <div style='background-color: #f4f4f4; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <code style='font-size: 16px; font-weight: bold;'>{resetToken}</code>
                </div>
                <p>This token will expire in 1 hour.</p>
                <p>If you didn't request this password reset, please ignore this email.</p>
                <br>
                <p>Best regards,<br>Volcanion PM Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationToken, CancellationToken cancellationToken = default)
    {
        var subject = "Confirm Your Email - Volcanion PM";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Email Confirmation</h2>
                <p>Hello {userName},</p>
                <p>Thank you for registering with Volcanion PM!</p>
                <p>Please use the following token to confirm your email address:</p>
                <div style='background-color: #f4f4f4; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <code style='font-size: 16px; font-weight: bold;'>{confirmationToken}</code>
                </div>
                <p>This token will expire in 24 hours.</p>
                <br>
                <p>Best regards,<br>Volcanion PM Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
        _logger.LogInformation("Email confirmation sent to {Email}", toEmail);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Volcanion PM!";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Welcome to Volcanion PM!</h2>
                <p>Hello {userName},</p>
                <p>Your account has been successfully created.</p>
                <p>You can now start managing your projects efficiently with our platform.</p>
                <br>
                <p>Best regards,<br>Volcanion PM Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
        _logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Preparing to send email to {Email} with subject: {Subject}", toEmail, subject);

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                Timeout = 30000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}. Error: {Error}", toEmail, ex.Message);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
