using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class RiskRepository : Repository<Risk>, IRiskRepository
{
    public RiskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Risk>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.Probability * r.Impact)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Risk>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .Where(r => r.OwnerId == ownerId)
            .OrderByDescending(r => r.Probability * r.Impact)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Risk>> GetHighPriorityRisksAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .Where(r => r.ProjectId == projectId && r.Level == RiskLevel.High && r.Status != RiskStatus.Resolved)
            .OrderByDescending(r => r.Probability * r.Impact)
            .ToListAsync(cancellationToken);
    }
}
