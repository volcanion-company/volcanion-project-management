using Microsoft.AspNetCore.Authorization;

namespace VolcanionPM.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for resource ownership.
/// Users can access/modify a resource if they are:
/// - The resource owner (CreatedBy matches UserId)
/// - An Admin
/// </summary>
public class IsResourceOwnerRequirement : IAuthorizationRequirement
{
}
