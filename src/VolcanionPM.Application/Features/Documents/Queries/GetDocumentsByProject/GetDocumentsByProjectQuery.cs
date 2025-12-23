using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentsByProject;

public record GetDocumentsByProjectQuery(Guid ProjectId) : IRequest<Result<List<DocumentDto>>>;
