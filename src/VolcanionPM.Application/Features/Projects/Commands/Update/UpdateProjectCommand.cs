using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;
using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Projects.Commands.Update;

public record UpdateProjectCommand : IRequest<Result<ProjectDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectPriority Priority { get; init; }
    public decimal? BudgetAmount { get; init; }
    public string? BudgetCurrency { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string UpdatedBy { get; init; } = "System";
}
