using Microsoft.EntityFrameworkCore;
using VolcanionPM.Application.Common.Interfaces;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.Infrastructure.Repositories;

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TimeEntry>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(te => te.User)
            .Include(te => te.Task)
            .Where(te => te.UserId == userId)
            .OrderByDescending(te => te.Date)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeEntry>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(te => te.User)
            .Include(te => te.Task)
            .Where(te => te.TaskId == taskId)
            .OrderByDescending(te => te.Date)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeEntry>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(te => te.User)
            .Include(te => te.Task)
            .Where(te => te.Task.ProjectId == projectId)
            .OrderByDescending(te => te.Date)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(te => te.User)
            .Include(te => te.Task)
            .Where(te => te.UserId == userId && te.Date >= startDate && te.Date <= endDate)
            .OrderByDescending(te => te.Date)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalHoursByTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(te => te.TaskId == taskId)
            .SumAsync(te => te.Hours, cancellationToken);
    }
}
