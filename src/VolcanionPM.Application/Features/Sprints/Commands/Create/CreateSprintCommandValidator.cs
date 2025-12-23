using FluentValidation;

namespace VolcanionPM.Application.Features.Sprints.Commands.Create;

public class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sprint name is required")
            .MaximumLength(200).WithMessage("Sprint name must not exceed 200 characters");

        RuleFor(x => x.SprintNumber)
            .GreaterThan(0).WithMessage("Sprint number must be positive");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.TotalStoryPoints)
            .GreaterThan(0).When(x => x.TotalStoryPoints.HasValue)
            .WithMessage("Total story points must be positive");
    }
}
