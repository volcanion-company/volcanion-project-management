using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// Organization Aggregate Root - Represents a company or team using the system
/// </summary>
public class Organization : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Website { get; private set; }
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? SubscriptionExpiryDate { get; private set; }
    
    // Navigation properties
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<Project> _projects = new();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    // Private constructor for EF Core
    private Organization() { }

    public static Organization Create(
        string name,
        string? description = null,
        string? website = null,
        Address? address = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required", nameof(name));

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Website = website?.Trim(),
            Address = address,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        organization.AddDomainEvent(new OrganizationCreatedEvent(organization.Id, organization.Name));
        return organization;
    }

    public void UpdateDetails(string name, string? description, string? website, Address? address, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Website = website?.Trim();
        Address = address;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLogo(string logoUrl, string updatedBy)
    {
        LogoUrl = logoUrl;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        UpdatedBy = deactivatedBy;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrganizationDeactivatedEvent(Id, Name));
    }

    public void Activate(string activatedBy)
    {
        IsActive = true;
        UpdatedBy = activatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSubscriptionExpiry(DateTime expiryDate, string updatedBy)
    {
        SubscriptionExpiryDate = expiryDate;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Domain Events
public record OrganizationCreatedEvent(Guid OrganizationId, string OrganizationName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record OrganizationDeactivatedEvent(Guid OrganizationId, string OrganizationName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
