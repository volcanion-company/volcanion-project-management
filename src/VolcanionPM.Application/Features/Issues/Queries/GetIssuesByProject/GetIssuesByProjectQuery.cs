using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Issues.DTOs;

namespace VolcanionPM.Application.Features.Issues.Queries.GetIssuesByProject;

public record GetIssuesByProjectQuery(Guid ProjectId) : IRequest<Result<List<IssueDto>>>;
