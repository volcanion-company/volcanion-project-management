# 📊 ENTITY REFERENCE GUIDE

Complete reference for all domain entities, their properties, methods, and relationships.

---

## 🎯 Aggregate Roots (3)

### 1. Organization
**File**: `Domain/Entities/Organization.cs`  
**Purpose**: Multi-tenant organization management

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Organization name |
| Description | string? | Optional description |
| LogoUrl | string? | Logo image URL |
| Website | string? | Organization website |
| Address | Address? | Physical address (Value Object) |
| IsActive | bool | Active status |
| SubscriptionExpiryDate | DateTime? | Subscription end date |

#### Relationships
- **Users** (1:N) → User[]
- **Projects** (1:N) → Project[]

#### Key Methods
- `Create()` - Factory method
- `UpdateDetails()` - Update organization info
- `SetLogo()` - Set logo URL
- `Deactivate()` - Deactivate organization
- `Activate()` - Reactivate organization
- `SetSubscriptionExpiry()` - Update subscription

#### Domain Events
- `OrganizationCreatedEvent`
- `OrganizationDeactivatedEvent`

---

### 2. User
**File**: `Domain/Entities/User.cs`  
**Purpose**: System user with authentication

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| FirstName | string | User's first name |
| LastName | string | User's last name |
| Email | Email | Email (Value Object) |
| PasswordHash | string | Hashed password |
| PhoneNumber | string? | Optional phone |
| AvatarUrl | string? | Profile picture URL |
| IsActive | bool | Active status |
| EmailConfirmed | bool | Email verified |
| LastLoginAt | DateTime? | Last login timestamp |
| RefreshToken | string? | JWT refresh token |
| RefreshTokenExpiryDate | DateTime? | Token expiry |
| OrganizationId | Guid | Parent organization |
| Role | UserRole | User role (enum) |

#### Relationships
- **Organization** (N:1) → Organization
- **OwnedProjects** (1:N) → Project[]
- **AssignedTasks** (1:N) → ProjectTask[]
- **TimeEntries** (1:N) → TimeEntry[]

#### Key Methods
- `Create()` - Factory method
- `GetFullName()` - Returns full name
- `UpdateProfile()` - Update user details
- `ChangePassword()` - Update password
- `ConfirmEmail()` - Mark email confirmed
- `SetAvatar()` - Set avatar URL
- `UpdateRefreshToken()` - Store refresh token
- `RevokeRefreshToken()` - Remove refresh token
- `RecordLogin()` - Update last login
- `ChangeRole()` - Change user role
- `Deactivate()` - Deactivate user
- `Activate()` - Reactivate user

#### Domain Events
- `UserCreatedEvent`
- `UserPasswordChangedEvent`
- `UserRoleChangedEvent`

---

### 3. Project
**File**: `Domain/Entities/Project.cs`  
**Purpose**: Main project aggregate root

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Project name |
| Code | string | Unique code (e.g., "PROJ-001") |
| Description | string? | Project description |
| Status | ProjectStatus | Current status (enum) |
| Priority | ProjectPriority | Priority level (enum) |
| DateRange | DateRange | Start/end dates (Value Object) |
| Budget | Money | Project budget (Value Object) |
| ProgressPercentage | decimal | Completion % (0-100) |
| OrganizationId | Guid | Parent organization |
| ProjectManagerId | Guid | Project manager user |

#### Relationships
- **Organization** (N:1) → Organization
- **ProjectManager** (N:1) → User
- **Tasks** (1:N) → ProjectTask[]
- **Sprints** (1:N) → Sprint[]
- **Risks** (1:N) → Risk[]
- **Issues** (1:N) → Issue[]
- **Documents** (1:N) → Document[]
- **ResourceAllocations** (1:N) → ResourceAllocation[]

#### Key Methods
- `Create()` - Factory method
- `UpdateDetails()` - Update project info
- `ChangeStatus()` - Change status
- `StartProject()` - Move to Active
- `CompleteProject()` - Mark complete
- `PutOnHold()` - Pause project
- `ResumeProject()` - Resume from hold
- `CancelProject()` - Cancel project
- `ChangeProjectManager()` - Reassign PM
- `UpdateProgress()` - Update completion %
- `AddTask()` - Add task to project
- `AddSprint()` - Add sprint to project

#### Status Workflow
```
Planning → Active → Completed
   ↓         ↓
   → OnHold →
   ↓
   → Cancelled
```

#### Domain Events
- `ProjectCreatedEvent`
- `ProjectUpdatedEvent`
- `ProjectStatusChangedEvent`
- `ProjectManagerChangedEvent`

---

## 📝 Child Entities (8)

### 4. ProjectTask
**File**: `Domain/Entities/ProjectTask.cs`  
**Purpose**: Work item in a project

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Title | string | Task title |
| Code | string | Unique code (e.g., "TASK-001") |
| Description | string? | Task description |
| Type | TaskType | Task type (enum) |
| Status | TaskStatus | Current status (enum) |
| Priority | TaskPriority | Priority level (enum) |
| StoryPoints | int? | Story points (optional) |
| EstimatedHours | decimal | Estimated effort |
| ActualHours | decimal | Actual time spent |
| DueDate | DateTime? | Due date (optional) |
| CompletedAt | DateTime? | Completion timestamp |
| ParentTaskId | Guid? | Parent task (for subtasks) |
| ProjectId | Guid | Parent project |
| AssignedToId | Guid? | Assigned user |
| SprintId | Guid? | Current sprint |

#### Relationships
- **Project** (N:1) → Project
- **AssignedTo** (N:1) → User?
- **Sprint** (N:1) → Sprint?
- **ParentTask** (N:1) → ProjectTask?
- **SubTasks** (1:N) → ProjectTask[]
- **TimeEntries** (1:N) → TimeEntry[]
- **Comments** (1:N) → TaskComment[]

#### Key Methods
- `Create()` - Factory method
- `UpdateDetails()` - Update task info
- `ChangeStatus()` - Move through workflow
- `AssignTo()` - Assign to user
- `Unassign()` - Remove assignment
- `AddToSprint()` - Add to sprint
- `RemoveFromSprint()` - Remove from sprint
- `RecordTime()` - Add actual hours
- `StartWork()` - Begin work
- `MarkAsComplete()` - Complete task
- `Block()` - Block task with reason
- `Unblock()` - Unblock task

#### Status Workflow
```
Backlog → ToDo → InProgress → InReview → Testing → Done
                     ↓
                  Blocked
```

#### Domain Events
- `TaskCreatedEvent`
- `TaskStatusChangedEvent`
- `TaskAssignedEvent`
- `TaskUnassignedEvent`
- `TaskBlockedEvent`

---

### 5. Sprint
**File**: `Domain/Entities/Sprint.cs`  
**Purpose**: Agile sprint/iteration

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Sprint name |
| Goal | string? | Sprint goal |
| DateRange | DateRange | Start/end dates (Value Object) |
| Status | SprintStatus | Current status (enum) |
| SprintNumber | int | Sprint number |
| TotalStoryPoints | int? | Planned story points |
| CompletedStoryPoints | int? | Actual completed points |
| ProjectId | Guid | Parent project |

#### Relationships
- **Project** (N:1) → Project
- **Tasks** (1:N) → ProjectTask[]

#### Key Methods
- `Create()` - Factory method
- `UpdateDetails()` - Update sprint info
- `Start()` - Begin sprint
- `Complete()` - End sprint with metrics
- `Cancel()` - Cancel sprint
- `AddTask()` - Add task to sprint
- `RemoveTask()` - Remove task from sprint
- `GetCompletedTaskCount()` - Count done tasks
- `GetTotalTaskCount()` - Count all tasks
- `GetCompletionPercentage()` - Calculate progress

#### Domain Events
- `SprintCreatedEvent`
- `SprintStartedEvent`
- `SprintCompletedEvent`
- `SprintCancelledEvent`

---

### 6. TimeEntry
**File**: `Domain/Entities/TimeEntry.cs`  
**Purpose**: Time tracking record

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| UserId | Guid | User who logged time |
| TaskId | Guid | Task time was logged on |
| Hours | decimal | Hours worked |
| Type | TimeEntryType | Entry type (enum) |
| Date | DateTime | Date of work |
| Description | string? | Optional description |
| IsBillable | bool | Billable to client |

#### Relationships
- **User** (N:1) → User
- **Task** (N:1) → ProjectTask

#### Key Methods
- `Create()` - Factory method
- `Update()` - Update entry details

#### Validation Rules
- Hours > 0
- Hours ≤ 24
- Valid date

#### Domain Events
- `TimeEntryLoggedEvent`

---

### 7. Risk
**File**: `Domain/Entities/Risk.cs`  
**Purpose**: Project risk management

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Title | string | Risk title |
| Description | string | Risk description |
| Level | RiskLevel | Risk level (enum) |
| Status | RiskStatus | Current status (enum) |
| Probability | decimal | Probability 0-100 |
| Impact | decimal | Impact 0-100 |
| MitigationStrategy | string? | Mitigation plan |
| IdentifiedDate | DateTime? | When identified |
| ResolvedDate | DateTime? | When resolved |
| ProjectId | Guid | Parent project |
| OwnerId | Guid? | Risk owner |

#### Relationships
- **Project** (N:1) → Project
- **Owner** (N:1) → User?

#### Key Methods
- `Create()` - Factory method
- `Update()` - Update risk details
- `ChangeStatus()` - Update status
- `AssignOwner()` - Assign risk owner
- `GetRiskScore()` - Calculate risk score

#### Risk Score Calculation
```
RiskScore = (Probability × Impact) / 100
```

#### Domain Events
- `RiskIdentifiedEvent`

---

### 8. Issue
**File**: `Domain/Entities/Issue.cs`  
**Purpose**: Bug/issue tracking

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Title | string | Issue title |
| Description | string | Issue description |
| Status | IssueStatus | Current status (enum) |
| Severity | IssueSeverity | Severity level (enum) |
| ResolvedDate | DateTime? | When resolved |
| Resolution | string? | Resolution details |
| ProjectId | Guid | Parent project |
| ReportedById | Guid? | Reporter user |
| AssignedToId | Guid? | Assigned user |

#### Relationships
- **Project** (N:1) → Project
- **ReportedBy** (N:1) → User?
- **AssignedTo** (N:1) → User?

#### Key Methods
- `Create()` - Factory method
- `Update()` - Update issue details
- `ChangeStatus()` - Update status
- `Resolve()` - Mark as resolved
- `Close()` - Close issue
- `Reopen()` - Reopen issue
- `AssignTo()` - Assign to user

#### Status Workflow
```
Open → InProgress → Resolved → Closed
  ↓                    ↓
  → → → → Reopened → → →
```

#### Domain Events
- `IssueCreatedEvent`
- `IssueResolvedEvent`

---

### 9. Document
**File**: `Domain/Entities/Document.cs`  
**Purpose**: Project document management

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Name | string | Document name |
| Description | string? | Document description |
| Type | DocumentType | Document type (enum) |
| FilePath | string | Storage path |
| FileExtension | string | File extension |
| FileSize | long | File size in bytes |
| Version | string? | Document version |
| ProjectId | Guid | Parent project |
| UploadedById | Guid | Uploader user |

#### Relationships
- **Project** (N:1) → Project
- **UploadedBy** (N:1) → User

#### Key Methods
- `Create()` - Factory method
- `Update()` - Update metadata
- `UpdateVersion()` - Update version
- `GetFileSizeFormatted()` - Human-readable size

#### Domain Events
- `DocumentUploadedEvent`

---

### 10. ResourceAllocation
**File**: `Domain/Entities/ResourceAllocation.cs`  
**Purpose**: Resource assignment to projects

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| UserId | Guid | Allocated user |
| ProjectId | Guid | Project |
| AllocationPeriod | DateRange | Time period (Value Object) |
| Type | ResourceAllocationType | Allocation type (enum) |
| AllocationPercentage | decimal | Allocation % (0-100) |
| HourlyRate | Money? | Hourly rate (Value Object) |
| Notes | string? | Additional notes |

#### Relationships
- **User** (N:1) → User
- **Project** (N:1) → Project

#### Key Methods
- `Create()` - Factory method
- `Update()` - Update allocation details
- `IsActiveOn()` - Check if active on date
- `IsCurrentlyActive()` - Check if active now

#### Domain Events
- `ResourceAllocatedEvent`

---

### 11. TaskComment
**File**: `Domain/Entities/TaskComment.cs`  
**Purpose**: Comments on tasks

#### Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | Guid | Unique identifier |
| Content | string | Comment text |
| IsEdited | bool | Edit flag |
| TaskId | Guid | Parent task |
| AuthorId | Guid | Comment author |

#### Relationships
- **Task** (N:1) → ProjectTask
- **Author** (N:1) → User

#### Key Methods
- `Create()` - Factory method
- `Update()` - Edit comment

#### Domain Events
- `TaskCommentAddedEvent`

---

## 🎨 Value Objects (4)

### Email
**File**: `Domain/ValueObjects/Email.cs`

```csharp
var email = Email.Create("user@example.com");
```

- Validates email format
- Normalizes to lowercase
- Immutable

### Money
**File**: `Domain/ValueObjects/Money.cs`

```csharp
var budget = Money.Create(50000, "USD");
var cost = Money.Create(25000, "USD");
var remaining = budget.Subtract(cost);
```

- Amount + currency
- Mathematical operations
- Currency validation

### DateRange
**File**: `Domain/ValueObjects/DateRange.cs`

```csharp
var period = DateRange.Create(startDate, endDate);
var duration = period.DurationInDays;
var overlaps = period.Overlaps(otherPeriod);
```

- Start and end dates
- Validation
- Overlap detection

### Address
**File**: `Domain/ValueObjects/Address.cs`

```csharp
var address = Address.Create(street, city, state, country, postalCode);
```

- Complete address structure
- Validation

---

## 📊 Complete Relationship Diagram

```
Organization
├── Users (1:N)
└── Projects (1:N)
    ├── ProjectManager (N:1 User)
    ├── Tasks (1:N)
    │   ├── AssignedTo (N:1 User)
    │   ├── Sprint (N:1 Sprint)
    │   ├── ParentTask (N:1 ProjectTask)
    │   ├── SubTasks (1:N ProjectTask)
    │   ├── TimeEntries (1:N)
    │   └── Comments (1:N)
    ├── Sprints (1:N)
    │   └── Tasks (1:N ProjectTask)
    ├── Risks (1:N)
    │   └── Owner (N:1 User)
    ├── Issues (1:N)
    │   ├── ReportedBy (N:1 User)
    │   └── AssignedTo (N:1 User)
    ├── Documents (1:N)
    │   └── UploadedBy (N:1 User)
    └── ResourceAllocations (1:N)
        └── User (N:1 User)
```

---

## 🔢 Statistics

- **Total Entities**: 11
- **Aggregate Roots**: 3
- **Child Entities**: 8
- **Value Objects**: 4
- **Domain Events**: 25+
- **Enumerations**: 13
- **Business Methods**: 100+

---

**Last Updated**: December 23, 2025  
**For**: Volcanion Project Management System  
**Status**: Phase 2 Complete
