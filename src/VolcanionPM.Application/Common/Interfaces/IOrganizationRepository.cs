using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Organization>> GetActiveOrganizationsAsync(CancellationToken cancellationToken = default);
}
