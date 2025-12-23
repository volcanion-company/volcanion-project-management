using FluentValidation;

namespace VolcanionPM.Application.Features.Projects.Commands.Update;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");

        RuleFor(x => x.BudgetAmount)
            .GreaterThanOrEqualTo(0).When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget amount must be non-negative");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
