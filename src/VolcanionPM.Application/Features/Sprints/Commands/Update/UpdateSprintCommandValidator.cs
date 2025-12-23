using FluentValidation;

namespace VolcanionPM.Application.Features.Sprints.Commands.Update;

public class UpdateSprintCommandValidator : AbstractValidator<UpdateSprintCommand>
{
    public UpdateSprintCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Sprint ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Sprint name is required")
            .MaximumLength(200).WithMessage("Sprint name must not exceed 200 characters");

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
