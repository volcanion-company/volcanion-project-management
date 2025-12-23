namespace VolcanionPM.Application.DTOs.Projects;

public record ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public Guid OrganizationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

public record CreateProjectDto
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public Guid OrganizationId { get; init; }
}

public record UpdateProjectDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
