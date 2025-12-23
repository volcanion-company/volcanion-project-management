using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Issues.Commands.Update;

public record UpdateIssueCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IssueSeverity Severity { get; init; }
}
