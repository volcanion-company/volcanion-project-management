using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface IResourceAllocationRepository : IRepository<ResourceAllocation>
{
    Task<IEnumerable<ResourceAllocation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceAllocation>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceAllocation>> GetActiveAllocationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceAllocation>> GetByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
