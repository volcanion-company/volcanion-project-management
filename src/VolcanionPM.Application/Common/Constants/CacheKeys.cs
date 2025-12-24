namespace VolcanionPM.Application.Common.Constants;

/// <summary>
/// Cache key conventions and TTL strategies
/// </summary>
public static class CacheKeys
{
    // Cache key prefixes
    public const string ProjectPrefix = "projects";
    public const string TaskPrefix = "tasks";
    public const string UserPrefix = "users";
    public const string OrganizationPrefix = "organizations";
    public const string SprintPrefix = "sprints";
    public const string TimeEntryPrefix = "timeentries";
    public const string RiskPrefix = "risks";
    public const string IssuePrefix = "issues";
    public const string DocumentPrefix = "documents";
    public const string ResourceAllocationPrefix = "resourceallocations";

    // Cache key builders
    public static string Project(Guid id) => $"{ProjectPrefix}:{id}";
    public static string ProjectByCode(string code) => $"{ProjectPrefix}:code:{code}";
    public static string ProjectsList(int page, int pageSize, string? filter = null) 
        => $"{ProjectPrefix}:list:page{page}:size{pageSize}:{filter ?? "all"}";

    public static string Task(Guid id) => $"{TaskPrefix}:{id}";
    public static string TasksList(Guid projectId, int page, int pageSize) 
        => $"{TaskPrefix}:project:{projectId}:page{page}:size{pageSize}";

    public static string User(Guid id) => $"{UserPrefix}:{id}";
    public static string UserByEmail(string email) => $"{UserPrefix}:email:{email}";
    public static string UsersList(int page, int pageSize) => $"{UserPrefix}:list:page{page}:size{pageSize}";

    public static string Organization(Guid id) => $"{OrganizationPrefix}:{id}";
    public static string OrganizationsList(int page, int pageSize) 
        => $"{OrganizationPrefix}:list:page{page}:size{pageSize}";

    public static string Sprint(Guid id) => $"{SprintPrefix}:{id}";
    public static string SprintsByProject(Guid projectId) => $"{SprintPrefix}:project:{projectId}";

    public static string TimeEntry(Guid id) => $"{TimeEntryPrefix}:{id}";
    public static string TimeEntriesByTask(Guid taskId) => $"{TimeEntryPrefix}:task:{taskId}";
    public static string TimeEntriesByUser(Guid userId, DateTime? start, DateTime? end) 
        => $"{TimeEntryPrefix}:user:{userId}:{start?.ToString("yyyy-MM-dd") ?? "all"}:{end?.ToString("yyyy-MM-dd") ?? "all"}";

    public static string Risk(Guid id) => $"{RiskPrefix}:{id}";
    public static string RisksByProject(Guid projectId) => $"{RiskPrefix}:project:{projectId}";

    public static string Issue(Guid id) => $"{IssuePrefix}:{id}";
    public static string IssuesByProject(Guid projectId) => $"{IssuePrefix}:project:{projectId}";

    public static string Document(Guid id) => $"{DocumentPrefix}:{id}";
    public static string DocumentsByProject(Guid projectId) => $"{DocumentPrefix}:project:{projectId}";

    public static string ResourceAllocation(Guid id) => $"{ResourceAllocationPrefix}:{id}";
    public static string ResourceAllocationsByProject(Guid projectId) 
        => $"{ResourceAllocationPrefix}:project:{projectId}";

    // TTL strategies (Time To Live)
    public static class Expiration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);      // Frequently changing data
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);    // Moderate changes
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);         // Rarely changing data
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);    // Static reference data
    }

    // Pattern builders for bulk invalidation
    public static string ProjectPattern() => $"{ProjectPrefix}:*";
    public static string TaskPattern() => $"{TaskPrefix}:*";
    public static string TasksByProjectPattern(Guid projectId) => $"{TaskPrefix}:project:{projectId}:*";
    public static string UserPattern() => $"{UserPrefix}:*";
    public static string OrganizationPattern() => $"{OrganizationPrefix}:*";
    public static string SprintsByProjectPattern(Guid projectId) => $"{SprintPrefix}:project:{projectId}:*";
}
