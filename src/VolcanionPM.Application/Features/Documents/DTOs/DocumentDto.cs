using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Documents.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public string? Version { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid UploadedById { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
