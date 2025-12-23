using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface ISprintRepository : IRepository<Sprint>
{
    Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetActiveSprintAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Sprint?> GetSprintWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
}
