using MediatR;
using VolcanionPM.Application.Common.Models;

namespace VolcanionPM.Application.Features.Organizations.Commands.Delete;

public record DeleteOrganizationCommand(Guid Id) : IRequest<Result<Unit>>;
