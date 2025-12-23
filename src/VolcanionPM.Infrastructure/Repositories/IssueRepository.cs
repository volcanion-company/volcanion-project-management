using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class IssueRepository : Repository<Issue>, IIssueRepository
{
    public IssueRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Issue>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .Include(i => i.ReportedBy)
            .Include(i => i.AssignedTo)
            .Include(i => i.Project)
            .Where(i => i.ProjectId == projectId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Issue>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .Include(i => i.ReportedBy)
            .Include(i => i.AssignedTo)
            .Include(i => i.Project)
            .Where(i => i.AssignedToId == userId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .Include(i => i.ReportedBy)
            .Include(i => i.AssignedTo)
            .Include(i => i.Project)
            .Where(i => i.Status == status && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Issue>> GetUnresolvedByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Issues
            .Include(i => i.ReportedBy)
            .Include(i => i.AssignedTo)
            .Include(i => i.Project)
            .Where(i => i.ProjectId == projectId && 
                        i.Status != IssueStatus.Resolved && 
                        i.Status != IssueStatus.Closed && 
                        !i.IsDeleted)
            .OrderBy(i => i.Severity)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
