using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Issues.Commands.Resolve;

public record ResolveIssueCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Resolution { get; init; } = string.Empty;
}
