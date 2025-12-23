using VolcanionPM.Domain.Entities;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Application.Common.Interfaces;

public interface ITaskRepository : IRepository<ProjectTask>
{
    Task<IEnumerable<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectTask>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectTask>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectTask>> GetByStatusAsync(Guid projectId, TaskStatus status, CancellationToken cancellationToken = default);
}
