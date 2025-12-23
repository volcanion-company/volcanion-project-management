using FluentValidation;

namespace VolcanionPM.Application.Features.Projects.Commands.Create;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Project code is required")
            .MaximumLength(20).WithMessage("Project code must not exceed 20 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Project code must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required")
            .Must(p => new[] { "Low", "Medium", "High", "Critical" }.Contains(p))
            .WithMessage("Priority must be Low, Medium, High, or Critical");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0).WithMessage("Budget amount must be greater than 0")
            .When(x => x.BudgetAmount.HasValue);
    }
}
