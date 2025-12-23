using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Documents.DTOs;

namespace VolcanionPM.Application.Features.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery(Guid Id) : IRequest<Result<DocumentDto>>;
