using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProjectManager)
            .Where(p => EF.Property<Guid>(p, "OrganizationId") == organizationId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetProjectWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Organization)
            .Include(p => p.ProjectManager)
            .AsSplitQuery()  // Prevents cartesian explosion
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
