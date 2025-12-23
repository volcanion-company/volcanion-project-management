using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Documents.Commands.Update;

public record UpdateDocumentCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DocumentType Type { get; init; }
}
