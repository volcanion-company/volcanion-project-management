using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Application.Features.Organizations.Commands.Create;

public record CreateOrganizationCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Website { get; init; }
    public AddressDto? Address { get; init; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
