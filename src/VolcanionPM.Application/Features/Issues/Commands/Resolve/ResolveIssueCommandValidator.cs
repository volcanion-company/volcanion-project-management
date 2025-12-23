using FluentValidation;

namespace VolcanionPM.Application.Features.Issues.Commands.Resolve;

public class ResolveIssueCommandValidator : AbstractValidator<ResolveIssueCommand>
{
    public ResolveIssueCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Issue ID is required");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required")
            .MaximumLength(1000).WithMessage("Resolution must not exceed 1000 characters");
    }
}
