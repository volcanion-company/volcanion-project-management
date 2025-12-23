using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Issues.Commands.Delete;

public record DeleteIssueCommand(Guid Id) : IRequest<Result<Unit>>;
