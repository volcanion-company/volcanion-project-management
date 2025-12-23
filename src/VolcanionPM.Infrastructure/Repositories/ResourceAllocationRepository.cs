using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class ResourceAllocationRepository : Repository<ResourceAllocation>, IResourceAllocationRepository
{
    public ResourceAllocationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ResourceAllocation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Include(r => r.User)
            .Include(r => r.Project)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.AllocationPeriod.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Include(r => r.User)
            .Include(r => r.Project)
            .Where(r => r.ProjectId == projectId && !r.IsDeleted)
            .OrderByDescending(r => r.AllocationPeriod.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetActiveAllocationsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.ResourceAllocations
            .Include(r => r.User)
            .Include(r => r.Project)
            .Where(r => !r.IsDeleted &&
                        r.AllocationPeriod.StartDate <= today &&
                        r.AllocationPeriod.EndDate >= today)
            .OrderBy(r => r.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetByUserAndDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Include(r => r.User)
            .Include(r => r.Project)
            .Where(r => r.UserId == userId && 
                        !r.IsDeleted &&
                        r.AllocationPeriod.StartDate <= endDate &&
                        r.AllocationPeriod.EndDate >= startDate)
            .OrderBy(r => r.AllocationPeriod.StartDate)
            .ToListAsync(cancellationToken);
    }
}
