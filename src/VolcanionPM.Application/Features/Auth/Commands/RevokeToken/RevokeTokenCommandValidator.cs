using FluentValidation;

namespace VolcanionPM.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address is required");
    }
}
