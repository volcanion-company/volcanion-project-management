using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.DTOs.Auth;

namespace VolcanionPM.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
