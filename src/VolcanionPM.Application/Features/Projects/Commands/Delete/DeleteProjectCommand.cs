using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Projects.Commands.Delete;

public record DeleteProjectCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string DeletedBy { get; init; } = "System";
}
