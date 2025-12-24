using VolcanionPM.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace VolcanionPM.Infrastructure.Services;

/// <summary>
/// Password hashing service using BCrypt
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            // Try both Verify methods
            var enhancedResult = BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
            var normalResult = BCrypt.Net.BCrypt.Verify(password, hash);
            return enhancedResult || normalResult;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
