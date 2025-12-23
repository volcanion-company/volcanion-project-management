using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Risks.DTOs;

public class RiskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskLevel Level { get; set; }
    public RiskStatus Status { get; set; }
    public decimal Probability { get; set; }
    public decimal Impact { get; set; }
    public decimal RiskScore { get; set; }
    public string? MitigationStrategy { get; set; }
    public DateTime? IdentifiedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
