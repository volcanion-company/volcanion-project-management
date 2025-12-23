using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Documents.Commands.Delete;

public record DeleteDocumentCommand(Guid Id) : IRequest<Result<Unit>>;
