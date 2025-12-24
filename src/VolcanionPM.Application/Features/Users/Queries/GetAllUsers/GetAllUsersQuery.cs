using MediatR;
using VolcanionPM.Application.Common.Models;
using VolcanionPM.Application.Features.Users.DTOs;

namespace VolcanionPM.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQuery : PagedQuery, IRequest<Result<PagedResult<UserDto>>>
{
    public Guid? OrganizationId { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "createdat";
    public string SortOrder { get; set; } = "desc";
}
