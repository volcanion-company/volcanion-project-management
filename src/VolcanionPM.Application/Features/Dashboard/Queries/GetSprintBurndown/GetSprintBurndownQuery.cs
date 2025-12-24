using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Dashboard.Queries.GetSprintBurndown;

public class GetSprintBurndownQuery : IRequest<Result<SprintBurndownDto>>
{
    public Guid SprintId { get; set; }
}
