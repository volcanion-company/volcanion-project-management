using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Organizations.Commands.Create;

namespace VolcanionPM.Application.Features.Organizations.Commands.Update;

public record UpdateOrganizationCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Website { get; init; }
    public AddressDto? Address { get; init; }
    public bool IsActive { get; init; }
}
