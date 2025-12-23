using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Document Entity - Represents a document attached to a project
/// </summary>
public class Document : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DocumentType Type { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public long FileSize { get; private set; } // in bytes
    public string? Version { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid UploadedById { get; private set; }
    public User UploadedBy { get; private set; } = null!;

    // Private constructor for EF Core
    private Document() { }

    public static Document Create(
        string name,
        DocumentType type,
        string filePath,
        string fileExtension,
        long fileSize,
        Guid projectId,
        Guid uploadedById,
        string? description = null,
        string? version = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Document name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSize));

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            FilePath = filePath,
            FileExtension = fileExtension.TrimStart('.').ToLowerInvariant(),
            FileSize = fileSize,
            ProjectId = projectId,
            UploadedById = uploadedById,
            Version = version,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        document.AddDomainEvent(new DocumentUploadedEvent(document.Id, name, projectId));
        return document;
    }

    public void Update(string name, string? description, DocumentType type, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Document name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateVersion(string version, string updatedBy)
    {
        Version = version;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFileSizeFormatted()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = FileSize;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public record DocumentUploadedEvent(Guid DocumentId, string DocumentName, Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
