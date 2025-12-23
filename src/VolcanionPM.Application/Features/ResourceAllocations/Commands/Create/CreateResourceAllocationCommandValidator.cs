using FluentValidation;

namespace VolcanionPM.Application.Features.ResourceAllocations.Commands.Create;

public class CreateResourceAllocationCommandValidator : AbstractValidator<CreateResourceAllocationCommand>
{
    public CreateResourceAllocationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required");

        RuleFor(x => x.AllocationPercentage)
            .InclusiveBetween(0, 100).WithMessage("Allocation percentage must be between 0 and 100");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid allocation type");

        RuleFor(x => x.HourlyRateAmount)
            .GreaterThan(0).When(x => x.HourlyRateAmount.HasValue)
            .WithMessage("Hourly rate must be positive");

        RuleFor(x => x.HourlyRateCurrency)
            .NotEmpty().When(x => x.HourlyRateAmount.HasValue)
            .WithMessage("Currency is required when hourly rate is specified")
            .Length(3).When(x => !string.IsNullOrWhiteSpace(x.HourlyRateCurrency))
            .WithMessage("Currency must be 3 characters (ISO 4217)");
    }
}
