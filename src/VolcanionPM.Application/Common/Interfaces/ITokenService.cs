using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

/// <summary>
/// Token service interface for JWT operations
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
}
