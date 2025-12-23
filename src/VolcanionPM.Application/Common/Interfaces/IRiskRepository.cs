using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IRiskRepository : IRepository<Risk>
{
    Task<IEnumerable<Risk>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Risk>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Risk>> GetHighPriorityRisksAsync(Guid projectId, CancellationToken cancellationToken = default);
}
