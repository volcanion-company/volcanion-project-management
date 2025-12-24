using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetProjectStatistics;

public class GetProjectStatisticsQuery : IRequest<Result<ProjectStatisticsDto>>
{
    public Guid? OrganizationId { get; set; }
}
