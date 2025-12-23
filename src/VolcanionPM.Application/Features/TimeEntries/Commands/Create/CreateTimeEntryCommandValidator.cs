using FluentValidation;

namespace VolcanionPM.Application.Features.TimeEntries.Commands.Create;

public class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Hours)
            .GreaterThan(0).WithMessage("Hours must be positive")
            .LessThanOrEqualTo(24).WithMessage("Cannot log more than 24 hours per entry");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid time entry type");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Cannot log time for future dates");
    }
}
