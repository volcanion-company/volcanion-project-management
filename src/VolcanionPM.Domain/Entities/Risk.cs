using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Risk Entity - Represents a project risk
/// </summary>
public class Risk : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RiskLevel Level { get; private set; }
    public RiskStatus Status { get; private set; }
    public decimal Probability { get; private set; } // 0-100
    public decimal Impact { get; private set; } // 0-100
    public string? MitigationStrategy { get; private set; }
    public DateTime? IdentifiedDate { get; private set; }
    public DateTime? ResolvedDate { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public Guid? OwnerId { get; private set; }
    public User? Owner { get; private set; }

    // Private constructor for EF Core
    private Risk() { }

    public static Risk Create(
        string title,
        string description,
        RiskLevel level,
        Guid projectId,
        decimal probability,
        decimal impact,
        Guid? ownerId = null,
        string? mitigationStrategy = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Risk title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Risk description is required", nameof(description));

        if (probability < 0 || probability > 100)
            throw new ArgumentException("Probability must be between 0 and 100", nameof(probability));

        if (impact < 0 || impact > 100)
            throw new ArgumentException("Impact must be between 0 and 100", nameof(impact));

        var risk = new Risk
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            Level = level,
            ProjectId = projectId,
            Probability = probability,
            Impact = impact,
            Status = RiskStatus.Identified,
            OwnerId = ownerId,
            MitigationStrategy = mitigationStrategy?.Trim(),
            IdentifiedDate = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        risk.AddDomainEvent(new RiskIdentifiedEvent(risk.Id, title, projectId, level));
        return risk;
    }

    public void Update(
        string title,
        string description,
        RiskLevel level,
        decimal probability,
        decimal impact,
        string? mitigationStrategy,
        string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Risk title is required", nameof(title));

        Title = title.Trim();
        Description = description.Trim();
        Level = level;
        Probability = probability;
        Impact = impact;
        MitigationStrategy = mitigationStrategy?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(RiskStatus newStatus, string updatedBy)
    {
        if (Status == newStatus)
            return;

        Status = newStatus;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == RiskStatus.Resolved)
        {
            ResolvedDate = DateTime.UtcNow;
        }
    }

    public void AssignOwner(Guid ownerId, string assignedBy)
    {
        OwnerId = ownerId;
        UpdatedBy = assignedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetRiskScore() => (Probability * Impact) / 100;
}

public record RiskIdentifiedEvent(Guid RiskId, string Title, Guid ProjectId, RiskLevel Level) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
