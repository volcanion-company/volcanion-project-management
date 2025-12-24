using Microsoft.AspNetCore.Authorization;

namespace VolcanionPM.API.Authorization.Requirements;

/// <summary>
/// Authorization requirement for assigning tasks.
/// Users can assign tasks if they are:
/// - The project's ProjectManager
/// - An Admin
/// </summary>
public class CanAssignTaskRequirement : IAuthorizationRequirement
{
}
