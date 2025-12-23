using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        // Note: OrganizationId is a shadow property, we need to use EF.Property
        return await _dbSet
            .Where(u => EF.Property<Guid>(u, "OrganizationId") == organizationId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.Value == email, cancellationToken);
    }
}
