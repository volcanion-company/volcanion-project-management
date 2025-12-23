using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IIssueRepository : IRepository<Issue>
{
    Task<IEnumerable<Issue>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Issue>> GetUnresolvedByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
