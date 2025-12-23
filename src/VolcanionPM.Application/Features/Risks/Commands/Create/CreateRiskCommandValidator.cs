using FluentValidation;

namespace VolcanionPM.Application.Features.Risks.Commands.Create;

public class CreateRiskCommandValidator : AbstractValidator<CreateRiskCommand>
{
    public CreateRiskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Risk title is required")
            .MaximumLength(200).WithMessage("Risk title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Risk description is required")
            .MaximumLength(2000).WithMessage("Risk description must not exceed 2000 characters");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid risk level");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100");

        RuleFor(x => x.Impact)
            .InclusiveBetween(0, 100).WithMessage("Impact must be between 0 and 100");
    }
}
