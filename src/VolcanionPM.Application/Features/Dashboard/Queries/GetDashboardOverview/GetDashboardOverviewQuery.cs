using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetDashboardOverview;

public class GetDashboardOverviewQuery : IRequest<Result<DashboardOverviewDto>>
{
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
}
