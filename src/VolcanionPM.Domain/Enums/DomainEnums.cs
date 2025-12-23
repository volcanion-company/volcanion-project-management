namespace VolcanionPM.Domain.Enums;

public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5,
    Archived = 6
}

public enum ProjectPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TaskStatus
{
    Backlog = 1,
    ToDo = 2,
    InProgress = 3,
    InReview = 4,
    Testing = 5,
    Done = 6,
    Blocked = 7,
    Cancelled = 8
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TaskType
{
    Task = 1,
    Story = 2,
    Bug = 3,
    Epic = 4,
    Feature = 5,
    Improvement = 6
}

public enum SprintStatus
{
    Planned = 1,
    Active = 2,
    Completed = 3,
    Cancelled = 4
}

public enum UserRole
{
    SystemAdmin = 1,
    OrganizationAdmin = 2,
    ProjectManager = 3,
    TeamLead = 4,
    Developer = 5,
    Designer = 6,
    Tester = 7,
    Viewer = 8
}

public enum IssueStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4,
    Reopened = 5
}

public enum IssueSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum RiskStatus
{
    Identified = 1,
    Analyzing = 2,
    Mitigating = 3,
    Resolved = 4,
    Accepted = 5
}

public enum ResourceAllocationType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Consultant = 4
}

public enum TimeEntryType
{
    Development = 1,
    Testing = 2,
    Design = 3,
    Meeting = 4,
    Review = 5,
    Documentation = 6,
    Other = 7
}

public enum DocumentType
{
    Requirements = 1,
    Design = 2,
    TechnicalSpec = 3,
    UserGuide = 4,
    TestPlan = 5,
    Other = 6
}
