using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetTeamVelocity;

public class GetTeamVelocityQuery : IRequest<Result<TeamVelocityDto>>
{
    public Guid ProjectId { get; set; }
    public int? LastNSprints { get; set; } = 5;
}
