using FluentValidation;

namespace VolcanionPM.Application.Features.Tasks.Commands.Update;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid task type");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0).WithMessage("Estimated hours must be greater than 0");

        RuleFor(x => x.StoryPoints)
            .GreaterThan(0).When(x => x.StoryPoints.HasValue)
            .WithMessage("Story points must be greater than 0");
    }
}
