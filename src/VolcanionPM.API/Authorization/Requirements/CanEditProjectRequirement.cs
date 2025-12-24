using Microsoft.AspNetCore.Authorization;

namespace VolcanionPM.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for editing a project.
/// Users can edit a project if they are:
/// - The project's ProjectManager
/// - An Admin
/// </summary>
public class CanEditProjectRequirement : IAuthorizationRequirement
{
}
