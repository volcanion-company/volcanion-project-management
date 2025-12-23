using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssueById;

public record GetIssueByIdQuery(Guid Id) : IRequest<Result<IssueDto>>;
