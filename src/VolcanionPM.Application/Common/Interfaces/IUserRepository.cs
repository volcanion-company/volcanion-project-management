using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
