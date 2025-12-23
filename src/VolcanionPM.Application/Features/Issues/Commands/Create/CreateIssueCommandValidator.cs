using FluentValidation;

namespace VolcanionPM.Application.Features.Issues.Commands.Create;

public class CreateIssueCommandValidator : AbstractValidator<CreateIssueCommand>
{
    public CreateIssueCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Issue title is required")
            .MaximumLength(200).WithMessage("Issue title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Issue description is required")
            .MaximumLength(2000).WithMessage("Issue description must not exceed 2000 characters");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Severity)
            .IsInEnum().WithMessage("Invalid issue severity");
    }
}
