using Microsoft.AspNetCore.Authorization;

namespace VolcanionPM.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for deleting a project.
/// Users can delete a project if they are:
/// - An Admin
/// </summary>
public class CanDeleteProjectRequirement : IAuthorizationRequirement
{
}
