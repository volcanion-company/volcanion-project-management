using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
