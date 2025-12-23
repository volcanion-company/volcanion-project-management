using FluentValidation;

namespace VolcanionPM.Application.Features.Issues.Commands.ChangeStatus;

public class ChangeIssueStatusCommandValidator : AbstractValidator<ChangeIssueStatusCommand>
{
    public ChangeIssueStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Issue ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid issue status");
    }
}
