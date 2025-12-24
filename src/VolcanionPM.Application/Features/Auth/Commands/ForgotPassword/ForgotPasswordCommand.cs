using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = string.Empty;
}
