using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Issues.Commands.ChangeStatus;

public record ChangeIssueStatusCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public IssueStatus Status { get; init; }
}
