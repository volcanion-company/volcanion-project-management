using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTaskStatistics;

public class GetTaskStatisticsQuery : IRequest<Result<TaskStatisticsDto>>
{
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
}
