using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Issues.Commands.Create;

public record CreateIssueCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
    public IssueSeverity Severity { get; init; }
    public Guid? ReportedById { get; init; }
    public Guid? AssignedToId { get; init; }
}
