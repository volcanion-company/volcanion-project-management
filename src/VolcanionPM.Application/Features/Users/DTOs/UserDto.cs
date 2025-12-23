using VolcanionPM.Domain.Enums;

namespace VolcanionPM.Application.Features.Users.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public Guid OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
