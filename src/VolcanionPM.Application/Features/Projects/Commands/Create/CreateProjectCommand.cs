using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Projects;

namespace VolcanionPM.Application.Features.Projects.Commands.Create;

public record CreateProjectCommand : IRequest<Result<ProjectDto>>
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
    public string CreatedBy { get; init; } = string.Empty;
}
