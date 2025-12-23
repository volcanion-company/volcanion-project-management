using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Tasks;

namespace VolcanionPM.Application.Features.Tasks.Queries.GetTasksByProject;

public record GetTasksByProjectQuery : IRequest<Result<List<TaskDto>>>
{
    public Guid ProjectId { get; init; }
}
