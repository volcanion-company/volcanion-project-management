using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Documents.Commands.Create;

public record CreateDocumentCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DocumentType Type { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public Guid ProjectId { get; init; }
    public Guid UploadedById { get; init; }
    public string? Version { get; init; }
}
