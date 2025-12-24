using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Normalize email to match Email Value Object normalization (lowercase + trim)
        var normalizedEmail = email.ToLowerInvariant().Trim();
        
        // Query using Email.Value property
        return await _dbSet
            .Where(u => u.Email.Value == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);
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
        // Normalize email to match Email Value Object normalization (lowercase + trim)
        var normalizedEmail = email.ToLowerInvariant().Trim();
        
        return await _dbSet
            .AnyAsync(u => u.Email.Value == normalizedEmail, cancellationToken);
    }
}
