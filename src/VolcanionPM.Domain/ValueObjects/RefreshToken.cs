using VolcanionPM.Domain.Common;

namespace VolcanionPM.Domain.ValueObjects;

public class RefreshToken : ValueObject
{
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    private RefreshToken(
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(string token, DateTime expiresAt, string createdByIp)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        if (string.IsNullOrWhiteSpace(createdByIp))
            throw new ArgumentException("IP address cannot be empty", nameof(createdByIp));

        return new RefreshToken(token, expiresAt, createdByIp);
    }

    public void Revoke(string ipAddress, string reason, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Token;
        yield return ExpiresAt;
        yield return CreatedAt;
        yield return CreatedByIp;
    }
}
