using FluentValidation;

namespace VolcanionPM.Application.Features.Documents.Commands.Update;

public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Document name is required")
            .MaximumLength(200).WithMessage("Document name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid document type");
    }
}
