namespace VolcanionPM.Application.Features.Organizations.DTOs;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public AddressDto? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
    public int UserCount { get; set; }
    public int ProjectCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
