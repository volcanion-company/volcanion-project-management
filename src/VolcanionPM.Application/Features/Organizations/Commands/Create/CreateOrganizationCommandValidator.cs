using FluentValidation;

namespace VolcanionPM.Application.Features.Organizations.Commands.Create;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters");

        When(x => x.Website != null, () =>
        {
            RuleFor(x => x.Website)
                .Must(BeAValidUrl).WithMessage("Website must be a valid URL");
        });
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
