# Phase 2: Domain Layer (DDD)

## ✅ Completed: December 23, 2025

---

## 📋 Overview

This phase implements the **Domain Layer**, which is the heart of the application. It contains all business logic, entities, value objects, domain events, and business rules following **Domain-Driven Design (DDD)** principles.

---

## 🏗️ Domain Layer Structure

```
VolcanionPM.Domain/
│
├── Common/
│   ├── BaseEntity.cs                    # Base class for all entities
│   ├── AggregateRoot.cs                 # Base for aggregate roots
│   ├── ValueObject.cs                   # Base for value objects
│   └── IDomainEvent.cs                  # Domain event interface
│
├── Entities/
│   ├── Organization.cs                  # Organization aggregate root
│   ├── User.cs                          # User aggregate root
│   ├── Project.cs                       # Project aggregate root
│   ├── ProjectTask.cs                   # Task entity
│   ├── Sprint.cs                        # Sprint entity
│   ├── TimeEntry.cs                     # Time tracking entity
│   ├── Risk.cs                          # Risk management entity
│   ├── Issue.cs                         # Issue tracking entity
│   ├── Document.cs                      # Document management entity
│   ├── ResourceAllocation.cs            # Resource allocation entity
│   └── TaskComment.cs                   # Task comment entity
│
├── ValueObjects/
│   ├── Email.cs                         # Email value object
│   ├── Money.cs                         # Money with currency
│   ├── DateRange.cs                     # Date range value object
│   └── Address.cs                       # Address value object
│
└── Enums/
    └── DomainEnums.cs                   # All domain enumerations
```

---

## 🎯 Core Concepts Implemented

### 1. **Aggregate Roots**

Aggregate roots are the main entities that control access to other entities within their boundary. Only aggregate roots can be directly retrieved from repositories.

#### Implemented Aggregate Roots:
- **Organization** - Root for organization-wide data
- **User** - Root for user identity and authentication
- **Project** - Root for project management

**Key Characteristics**:
- Extend `AggregateRoot` base class
- Have unique identity (Guid)
- Control consistency boundaries
- Raise domain events
- Enforce business rules

---

### 2. **Entities**

Entities are objects with identity that persist over time. They are accessed through their aggregate roots.

#### Core Entities Implemented:
1. **ProjectTask** - Work items in a project
2. **Sprint** - Agile sprint/iteration
3. **TimeEntry** - Time tracking records
4. **Risk** - Project risk management
5. **Issue** - Issue/bug tracking
6. **Document** - Project documents
7. **ResourceAllocation** - Resource assignment
8. **TaskComment** - Task discussions

**Entity Features**:
- Unique Id (Guid)
- Audit fields (Created, Updated, Deleted)
- Soft delete support
- Domain event support
- Rich business methods

---

### 3. **Value Objects**

Value objects are immutable objects defined by their attributes, not identity. Two value objects with same values are considered equal.

#### Implemented Value Objects:

**Email**
```csharp
var email = Email.Create("user@example.com");
// - Validates email format
// - Normalizes to lowercase
// - Immutable
```

**Money**
```csharp
var budget = Money.Create(50000, "USD");
var cost = Money.Create(25000, "USD");
var remaining = budget.Subtract(cost);
// - Amount with currency
// - Currency validation
// - Mathematical operations
```

**DateRange**
```csharp
var period = DateRange.Create(startDate, endDate);
var duration = period.DurationInDays;
var overlaps = period.Overlaps(otherPeriod);
// - Start and end dates
// - Validation
// - Overlap detection
```

**Address**
```csharp
var address = Address.Create(street, city, state, country, postalCode);
// - Complete address structure
// - Validation
// - Formatted output
```

---

### 4. **Domain Events**

Domain events represent something that happened in the domain that other parts of the system might care about.

#### Sample Domain Events:

```csharp
// Organization Events
OrganizationCreatedEvent
OrganizationDeactivatedEvent

// User Events
UserCreatedEvent
UserPasswordChangedEvent
UserRoleChangedEvent

// Project Events
ProjectCreatedEvent
ProjectStatusChangedEvent
ProjectManagerChangedEvent

// Task Events
TaskCreatedEvent
TaskStatusChangedEvent
TaskAssignedEvent
TaskBlockedEvent

// Sprint Events
SprintCreatedEvent
SprintStartedEvent
SprintCompletedEvent

// And many more...
```

**Event Structure**:
```csharp
public record ProjectCreatedEvent(
    Guid ProjectId, 
    string ProjectName, 
    string ProjectCode) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

---

### 5. **Domain Enumerations**

Clear, type-safe enumerations for domain concepts:

```csharp
// Project Management
ProjectStatus: Planning, Active, OnHold, Completed, Cancelled, Archived
ProjectPriority: Low, Medium, High, Critical

// Task Management
TaskStatus: Backlog, ToDo, InProgress, InReview, Testing, Done, Blocked, Cancelled
TaskPriority: Low, Medium, High, Critical
TaskType: Task, Story, Bug, Epic, Feature, Improvement

// Sprint
SprintStatus: Planned, Active, Completed, Cancelled

// User & Security
UserRole: SystemAdmin, OrganizationAdmin, ProjectManager, TeamLead, 
          Developer, Designer, Tester, Viewer

// Risk & Issue
RiskLevel: Low, Medium, High, Critical
RiskStatus: Identified, Analyzing, Mitigating, Resolved, Accepted
IssueStatus: Open, InProgress, Resolved, Closed, Reopened
IssueSeverity: Low, Medium, High, Critical

// Resource Management
ResourceAllocationType: FullTime, PartTime, Contract, Consultant
TimeEntryType: Development, Testing, Design, Meeting, Review, Documentation, Other

// Documents
DocumentType: Requirements, Design, TechnicalSpec, UserGuide, TestPlan, Other
```

---

## 🧬 Entity Details

### Organization Entity
**Purpose**: Represents a company or team using the system

**Key Features**:
- Multi-tenant support
- Subscription management
- User and project ownership
- Activation/deactivation

**Business Methods**:
- `Create()` - Factory method
- `UpdateDetails()` - Update organization info
- `Deactivate()` / `Activate()` - Manage status
- `SetSubscriptionExpiry()` - Subscription management

---

### User Entity
**Purpose**: System user with authentication and authorization

**Key Features**:
- Email-based authentication
- Role-based access control (RBAC)
- JWT refresh token support
- Profile management
- Last login tracking

**Business Methods**:
- `Create()` - Create new user
- `UpdateProfile()` - Update user details
- `ChangePassword()` - Password management
- `ConfirmEmail()` - Email verification
- `UpdateRefreshToken()` / `RevokeRefreshToken()` - Token management
- `ChangeRole()` - Role management
- `RecordLogin()` - Login tracking

---

### Project Entity
**Purpose**: Main project management aggregate root

**Key Features**:
- Project lifecycle management
- Budget tracking
- Progress monitoring
- Status workflow
- Project manager assignment

**Business Methods**:
- `Create()` - Create new project
- `UpdateDetails()` - Update project info
- `StartProject()` - Begin project work
- `CompleteProject()` - Mark as complete
- `PutOnHold()` / `ResumeProject()` - Pause/resume
- `CancelProject()` - Cancel project
- `ChangeProjectManager()` - Reassign PM
- `UpdateProgress()` - Track progress

**Workflow**:
```
Planning → Active → Completed
    ↓        ↓
    → OnHold →
    ↓
    → Cancelled
```

---

### ProjectTask Entity
**Purpose**: Work item in a project

**Key Features**:
- Hierarchical structure (parent/subtasks)
- Sprint assignment
- Time tracking
- User assignment
- Story point estimation
- Status workflow

**Business Methods**:
- `Create()` - Create new task
- `UpdateDetails()` - Update task info
- `ChangeStatus()` - Move through workflow
- `AssignTo()` / `Unassign()` - Assignment management
- `AddToSprint()` / `RemoveFromSprint()` - Sprint management
- `StartWork()` - Begin task
- `MarkAsComplete()` - Complete task
- `Block()` / `Unblock()` - Handle blockers

**Workflow**:
```
Backlog → ToDo → InProgress → InReview → Testing → Done
                     ↓
                  Blocked
```

---

### Sprint Entity
**Purpose**: Agile sprint/iteration management

**Key Features**:
- Time-boxed iterations
- Story point tracking
- Goal setting
- Task association
- Velocity calculation

**Business Methods**:
- `Create()` - Create new sprint
- `UpdateDetails()` - Update sprint info
- `Start()` - Begin sprint
- `Complete()` - End sprint with metrics
- `Cancel()` - Cancel sprint
- `AddTask()` / `RemoveTask()` - Task management
- `GetCompletionPercentage()` - Progress tracking

---

### TimeEntry Entity
**Purpose**: Time tracking for tasks

**Key Features**:
- Hour logging
- Billable vs non-billable
- Entry type classification
- Daily tracking

**Validation**:
- Max 24 hours per entry
- Positive hours only
- Date validation

---

### Risk Entity
**Purpose**: Project risk management

**Key Features**:
- Probability & impact scoring
- Risk level classification
- Mitigation strategy
- Owner assignment
- Status tracking

**Risk Score Calculation**:
```csharp
RiskScore = (Probability × Impact) / 100
```

---

### Issue Entity
**Purpose**: Bug and issue tracking

**Key Features**:
- Severity levels
- Status workflow
- Assignment tracking
- Resolution tracking

**Workflow**:
```
Open → InProgress → Resolved → Closed
  ↓                    ↓
  → → → → Reopened → → →
```

---

### Document Entity
**Purpose**: Project document management

**Key Features**:
- File metadata tracking
- Version control
- Document type classification
- File size tracking
- Upload user tracking

---

### ResourceAllocation Entity
**Purpose**: Resource assignment to projects

**Key Features**:
- Allocation percentage (0-100%)
- Time period tracking
- Hourly rate tracking
- Allocation type (Full-time, Part-time, etc.)

---

## 🎨 Design Patterns Applied

### 1. **Factory Pattern**
All entities use static factory methods:
```csharp
var project = Project.Create(name, code, orgId, managerId, dateRange, budget);
```

**Benefits**:
- Encapsulates creation logic
- Ensures valid state
- Clear intent
- Can raise domain events

---

### 2. **Rich Domain Model**
Entities have behavior, not just data:
```csharp
project.StartProject(userId);
task.Block("Waiting for API", userId);
sprint.Complete(userId);
```

**Benefits**:
- Business logic in domain
- Encapsulation
- Self-documenting code

---

### 3. **Domain Events**
Events capture important state changes:
```csharp
var project = Project.Create(...);
// Raises ProjectCreatedEvent

project.StartProject(userId);
// Raises ProjectStatusChangedEvent
```

**Benefits**:
- Loose coupling
- Audit trail
- Integration points
- Event sourcing ready

---

### 4. **Value Object Pattern**
Immutable value-based equality:
```csharp
var email1 = Email.Create("test@example.com");
var email2 = Email.Create("test@example.com");
// email1 == email2 → true
```

**Benefits**:
- Thread-safe
- No identity issues
- Clear semantics

---

## 🔒 Business Rules Enforced

### Organization Rules
✅ Name is required  
✅ Can only deactivate active organizations  
✅ Subscription expiry tracking  

### User Rules
✅ Email must be valid and unique  
✅ Password hash required  
✅ Role-based access control  
✅ Email confirmation flow  
✅ Refresh token expiration  

### Project Rules
✅ Project code must be unique  
✅ Can only start from Planning status  
✅ Can only complete Active projects  
✅ Can only pause Active projects  
✅ Cannot cancel completed projects  
✅ Budget must be positive  
✅ Date range must be valid  

### Task Rules
✅ Title and code required  
✅ Estimated hours ≥ 0  
✅ Can only block InProgress tasks  
✅ Story points optional but positive  
✅ Hierarchy validation (no circular references)  

### Sprint Rules
✅ Sprint number must be positive  
✅ Cannot update completed sprints  
✅ Cannot start before start date  
✅ Date range validation  
✅ Cannot add tasks to completed sprints  

### TimeEntry Rules
✅ Hours must be positive  
✅ Maximum 24 hours per entry  
✅ Valid date required  

### Risk Rules
✅ Probability 0-100  
✅ Impact 0-100  
✅ Title and description required  

### Issue Rules
✅ Title and description required  
✅ Status workflow validation  

---

## 📊 Domain Statistics

**Entities Implemented**: 11
- 3 Aggregate Roots (Organization, User, Project)
- 8 Child Entities

**Value Objects**: 4
- Email, Money, DateRange, Address

**Domain Events**: 25+

**Enumerations**: 13

**Business Methods**: 100+

---

## 🎯 DDD Best Practices Applied

✅ **Ubiquitous Language** - Method names match business terminology  
✅ **Aggregate Boundaries** - Clear consistency boundaries  
✅ **Encapsulation** - Private setters, business methods  
✅ **Immutability** - Value objects are immutable  
✅ **Domain Events** - Important changes captured  
✅ **Factory Methods** - Controlled entity creation  
✅ **Validation** - Business rules in domain  
✅ **No Anemic Model** - Rich behavior, not just getters/setters  

---

## 🔄 Entity Relationships

```
Organization
    ├── Users (1:N)
    └── Projects (1:N)
        ├── Tasks (1:N)
        │   ├── SubTasks (1:N)
        │   ├── TimeEntries (1:N)
        │   └── Comments (1:N)
        ├── Sprints (1:N)
        │   └── Tasks (N:M)
        ├── Risks (1:N)
        ├── Issues (1:N)
        ├── Documents (1:N)
        └── ResourceAllocations (1:N)

User
    ├── OwnedProjects (1:N)
    ├── AssignedTasks (1:N)
    ├── TimeEntries (1:N)
    └── ResourceAllocations (1:N)
```

---

## 🚀 What's Next?

With Phase 2 complete, we have:
✅ Complete domain model
✅ 11 entities with rich behavior
✅ 4 value objects
✅ 25+ domain events
✅ 13 enumerations
✅ Business rules enforced
✅ DDD patterns applied

**Phase 3** will implement the Infrastructure Layer with:
- WriteDbContext (EF Core)
- ReadDbContext (EF Core)
- Repository implementations
- Unit of Work
- PostgreSQL configuration
- Redis cache service

---

## 💡 Key Takeaways

1. **Domain is Independent** - No infrastructure concerns
2. **Rich Behavior** - Business logic lives in entities
3. **Type Safety** - Strong typing with value objects and enums
4. **Event-Driven** - Domain events for important changes
5. **Validation** - Business rules enforced at domain level
6. **Immutability** - Value objects prevent bugs
7. **Factory Pattern** - Controlled entity creation
8. **Audit Trail** - Automatic tracking of changes

---

**Generated**: December 23, 2025  
**Status**: ✅ Complete  
**Next Phase**: Phase 3 - Infrastructure Layer
