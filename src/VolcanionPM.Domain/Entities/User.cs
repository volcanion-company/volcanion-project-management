using VolcanionPM.Domain.Common;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Domain.Entities;

/// <summary>
/// User Aggregate Root - Represents a user in the system
/// </summary>
public class User : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryDate { get; private set; }
    
    // Organization relationship
    public Guid OrganizationId { get; private set; }
    public Organization Organization { get; private set; } = null!;

    // Role-based access
    public UserRole Role { get; private set; }

    // Navigation properties
    private readonly List<Project> _ownedProjects = new();
    public IReadOnlyCollection<Project> OwnedProjects => _ownedProjects.AsReadOnly();

    private readonly List<ProjectTask> _assignedTasks = new();
    public IReadOnlyCollection<ProjectTask> AssignedTasks => _assignedTasks.AsReadOnly();

    private readonly List<TimeEntry> _timeEntries = new();
    public IReadOnlyCollection<TimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    // Private constructor for EF Core
    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        Guid organizationId,
        UserRole role,
        string? phoneNumber = null,
        string createdBy = "System")
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            OrganizationId = organizationId,
            Role = role,
            PhoneNumber = phoneNumber?.Trim(),
            IsActive = true,
            EmailConfirmed = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.GetFullName(), user.Email.Value));
        return user;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id, Email.Value));
    }

    public void ConfirmEmail(string updatedBy)
    {
        EmailConfirmed = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvatar(string avatarUrl, string updatedBy)
    {
        AvatarUrl = avatarUrl;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRefreshToken(string refreshToken, DateTime expiryDate, string updatedBy)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryDate = expiryDate;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevokeRefreshToken(string updatedBy)
    {
        RefreshToken = null;
        RefreshTokenExpiryDate = null;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole, string updatedBy)
    {
        if (Role == newRole)
            return;

        var oldRole = Role;
        Role = newRole;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserRoleChangedEvent(Id, oldRole, newRole));
    }

    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        UpdatedBy = deactivatedBy;
        UpdatedAt = DateTime.UtcNow;
        RevokeRefreshToken(deactivatedBy);
    }

    public void Activate(string activatedBy)
    {
        IsActive = true;
        UpdatedBy = activatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Domain Events
public record UserCreatedEvent(Guid UserId, string FullName, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserPasswordChangedEvent(Guid UserId, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record UserRoleChangedEvent(Guid UserId, UserRole OldRole, UserRole NewRole) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
