using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTimeDistribution;

public class GetTimeDistributionQuery : IRequest<Result<TimeDistributionDto>>
{
    public Guid? UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
