using FluentValidation;

namespace VolcanionPM.Application.Features.Documents.Commands.Create;

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Document name is required")
            .MaximumLength(200).WithMessage("Document name must not exceed 200 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(500).WithMessage("File path must not exceed 500 characters");

        RuleFor(x => x.FileExtension)
            .NotEmpty().WithMessage("File extension is required")
            .MaximumLength(10).WithMessage("File extension must not exceed 10 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be positive")
            .LessThan(104857600).WithMessage("File size must not exceed 100MB"); // 100MB limit

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.UploadedById)
            .NotEmpty().WithMessage("Uploader ID is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid document type");
    }
}
