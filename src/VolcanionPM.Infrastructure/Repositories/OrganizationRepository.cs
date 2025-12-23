using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);
    }
}
