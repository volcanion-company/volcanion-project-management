using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class SprintRepository : Repository<Sprint>, ISprintRepository
{
    public SprintRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Sprint>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => EF.Property<Guid>(s, "ProjectId") == projectId)
            .OrderByDescending(s => s.DateRange.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sprint?> GetActiveSprintAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => EF.Property<Guid>(s, "ProjectId") == projectId && s.Status == SprintStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Sprint?> GetSprintWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
