using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Application.Common.Interfaces;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<IEnumerable<TimeEntry>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalHoursByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
}
