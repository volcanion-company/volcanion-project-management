using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class TaskRepository : Repository<ProjectTask>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => EF.Property<Guid>(t, "ProjectId") == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProjectTask>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AssignedToId == userId)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProjectTask>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.SprintId == sprintId)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProjectTask>> GetByStatusAsync(Guid projectId, TaskStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => EF.Property<Guid>(t, "ProjectId") == projectId && t.Status == status)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);
    }
}
