# Volcanion Project Management - System Flows

## 📋 Table of Contents

1. [Authentication Flow](#authentication-flow)
2. [CQRS Pattern Flow](#cqrs-pattern-flow)
3. [CRUD Operation Flow](#crud-operation-flow)
4. [Caching Strategy Flow](#caching-strategy-flow)
5. [Domain Event Flow](#domain-event-flow)
6. [Complete Request Lifecycle](#complete-request-lifecycle)
7. [Error Handling Flow](#error-handling-flow)
8. [Authorization Flow](#authorization-flow)
9. [Database Transaction Flow](#database-transaction-flow)
10. [Background Job Flow](#background-job-flow)

---

## 🔐 Authentication Flow

### 1. Registration Flow

```
User Request → API Controller → Command Handler → Validation
                                                      ↓
                                         Check Email Uniqueness (Database)
                                                      ↓
                                         Hash Password (BCrypt)
                                                      ↓
                                         Create User Entity (Domain)
                                                      ↓
                                         Save to Database (Repository)
                                                      ↓
                                         Raise UserRegisteredEvent
                                                      ↓
                                         Send Welcome Email (Event Handler)
                                                      ↓
                                         Return Success Response
```

**Implementation Details:**

```
POST /api/auth/register
├── AuthController.Register()
│   ├── RegisterUserCommand
│   │   ├── FirstName, LastName, Email, Password
│   │   └── OrganizationId, Role
│   └── Send to Mediator
│
├── RegisterUserCommandHandler
│   ├── Validate Command (FluentValidation)
│   │   ├── Email format
│   │   ├── Password strength (min 8 chars, uppercase, lowercase, number)
│   │   └── Required fields
│   ├── Check Email Uniqueness (UserRepository.GetByEmailAsync)
│   ├── Hash Password (BCrypt)
│   ├── Create User Entity
│   │   └── User.Create() - Domain method
│   ├── Save to Database (UnitOfWork.SaveChangesAsync)
│   └── Return Result<UserDto>
│
└── Domain Events
    └── UserRegisteredEvent
        └── SendWelcomeEmailEventHandler
            └── EmailService.SendAsync()
```

### 2. Login Flow

```
User Credentials → API Controller → Query Handler
                                           ↓
                          Find User by Email (Database)
                                           ↓
                          Verify Password (BCrypt)
                                           ↓
                          Check Account Status (Active/Locked)
                                           ↓
                          Generate JWT Access Token
                                           ↓
                          Generate Refresh Token
                                           ↓
                          Update Last Login (Database)
                                           ↓
                          Return Tokens + User Info
```

**Implementation Details:**

```
POST /api/auth/login
├── AuthController.Login()
│   ├── LoginCommand
│   │   ├── Email
│   │   └── Password
│   └── Send to Mediator
│
├── LoginCommandHandler
│   ├── Find User (UserRepository.GetByEmailAsync)
│   ├── Check Account Status
│   │   ├── IsActive == true
│   │   ├── LockoutEnd == null or < Now
│   │   └── FailedLoginAttempts < MaxAttempts (5)
│   ├── Verify Password
│   │   ├── BCrypt.Verify(password, user.PasswordHash)
│   │   └── If failed → Increment FailedLoginAttempts
│   ├── Generate Tokens
│   │   ├── Access Token (JWT - 60 minutes)
│   │   │   ├── Claims: UserId, Email, Role, OrganizationId
│   │   │   └── Signed with JWT_SECRET_KEY
│   │   └── Refresh Token (GUID - 7 days)
│   ├── Update User
│   │   ├── LastLoginAt = Now
│   │   ├── RefreshToken = newToken
│   │   ├── RefreshTokenExpiresAt = Now + 7 days
│   │   └── FailedLoginAttempts = 0
│   └── Return AuthenticationResponse
│       ├── AccessToken
│       ├── RefreshToken
│       ├── ExpiresAt
│       └── UserDto
│
└── Failed Login Handling
    └── If attempts >= 5 → Lock account for 15 minutes
```

### 3. Refresh Token Flow

```
Refresh Token → API Controller → Command Handler
                                        ↓
                     Find User by Refresh Token (Database)
                                        ↓
                     Validate Token (Not Expired)
                                        ↓
                     Generate New Access Token
                                        ↓
                     Generate New Refresh Token
                                        ↓
                     Update Tokens in Database
                                        ↓
                     Return New Tokens
```

**Implementation Details:**

```
POST /api/auth/refresh
├── AuthController.RefreshToken()
│   ├── RefreshTokenCommand
│   │   └── RefreshToken (GUID)
│   └── Send to Mediator
│
├── RefreshTokenCommandHandler
│   ├── Find User (UserRepository.GetByRefreshTokenAsync)
│   ├── Validate Token
│   │   ├── Token exists
│   │   ├── Token not expired (RefreshTokenExpiresAt > Now)
│   │   └── User is active
│   ├── Generate New Tokens
│   │   ├── New Access Token (JWT)
│   │   └── New Refresh Token (GUID)
│   ├── Update User
│   │   ├── RefreshToken = newToken
│   │   └── RefreshTokenExpiresAt = Now + 7 days
│   └── Return AuthenticationResponse
│
└── Error Cases
    ├── Invalid Token → 401 Unauthorized
    ├── Expired Token → 401 Unauthorized
    └── User Inactive → 403 Forbidden
```

### 4. Logout Flow

```
Access Token → API Controller → Command Handler
                                       ↓
                      Extract User from JWT
                                       ↓
                      Clear Refresh Token (Database)
                                       ↓
                      Return Success
```

---

## 🎯 CQRS Pattern Flow

### Command Flow (Write Operations)

```
HTTP Request → Controller → Command → Mediator → Command Handler
                                                        ↓
                                           Validation (FluentValidation)
                                                        ↓
                                           Load Aggregate (Repository)
                                                        ↓
                                           Business Logic (Domain)
                                                        ↓
                                           Modify Aggregate State
                                                        ↓
                                           Raise Domain Events
                                                        ↓
                                           Save Changes (UnitOfWork)
                                                        ↓
                                           Invalidate Cache
                                                        ↓
                                           Dispatch Domain Events
                                                        ↓
                                           Return Result<T>
```

**Example: Create Project Command**

```
POST /api/projects
├── ProjectsController.CreateProject()
│   ├── CreateProjectCommand
│   │   ├── Name, Description
│   │   ├── OrganizationId, ProjectManagerId
│   │   ├── StartDate, EndDate
│   │   └── Budget
│   └── Send to Mediator
│
├── CreateProjectCommandHandler
│   ├── Validate Command
│   │   ├── Name not empty
│   │   ├── End date > Start date
│   │   ├── Budget >= 0
│   │   └── Organization exists
│   ├── Check Permissions
│   │   └── User can create projects
│   ├── Create Project Entity
│   │   ├── Project.Create() - Factory method
│   │   ├── Set initial status = Planning
│   │   └── Calculate progress = 0%
│   ├── Save to Database
│   │   ├── _projectRepository.AddAsync(project)
│   │   └── _unitOfWork.SaveChangesAsync()
│   ├── Raise Domain Event
│   │   └── ProjectCreatedEvent
│   ├── Invalidate Cache
│   │   └── _cache.RemoveAsync($"projects:{organizationId}")
│   └── Return Result<ProjectDto>
│
└── Domain Event Handler
    └── ProjectCreatedEventHandler
        ├── Send notification to project manager
        ├── Create audit log entry
        └── Update organization statistics
```

### Query Flow (Read Operations)

```
HTTP Request → Controller → Query → Mediator → Query Handler
                                                      ↓
                                          Check Cache (Redis)
                                                      ↓
                                          If cached → Return from cache
                                                      ↓
                                          If not cached:
                                          └── Query Database (Dapper/EF Core)
                                          └── Map to DTO
                                          └── Cache Result (Redis)
                                          └── Return Result<T>
```

**Example: Get Project by ID Query**

```
GET /api/projects/{id}
├── ProjectsController.GetById()
│   ├── GetProjectByIdQuery
│   │   └── ProjectId
│   └── Send to Mediator
│
├── GetProjectByIdQueryHandler
│   ├── Check Cache
│   │   ├── Key: $"project:{id}"
│   │   └── If exists → Return cached ProjectDto
│   ├── Query Database
│   │   ├── _context.Projects
│   │   │   .Include(p => p.Organization)
│   │   │   .Include(p => p.ProjectManager)
│   │   │   .FirstOrDefaultAsync(p => p.Id == id)
│   │   └── If not found → Return NotFound
│   ├── Map to DTO
│   │   └── ProjectDto.FromEntity(project)
│   ├── Cache Result
│   │   ├── Key: $"project:{id}"
│   │   ├── Value: ProjectDto (JSON)
│   │   └── Expiration: 30 minutes
│   └── Return Result<ProjectDto>
│
└── Cache Invalidation
    └── On project update/delete
        └── _cache.RemoveAsync($"project:{id}")
```

---

## ✏️ CRUD Operation Flow

### Create Operation

```
Client Request
    ↓
API Controller (POST /api/resource)
    ↓
CreateResourceCommand
    ↓
Validation Behavior (FluentValidation)
    ↓
Authorization Check (Can create resource?)
    ↓
CreateResourceCommandHandler
    ├── Business Validation
    ├── Create Domain Entity
    ├── Repository.AddAsync()
    ├── UnitOfWork.SaveChangesAsync()
    └── Raise Domain Events
    ↓
Cache Invalidation (Clear list cache)
    ↓
Return 201 Created + Resource DTO
```

### Read Operation

```
Client Request
    ↓
API Controller (GET /api/resource/{id})
    ↓
GetResourceByIdQuery
    ↓
GetResourceByIdQueryHandler
    ├── Check Cache (Redis)
    │   └── Key: "resource:{id}"
    ├── If not cached:
    │   ├── Query Database
    │   ├── Map to DTO
    │   └── Cache Result
    └── Return Result
    ↓
Return 200 OK + Resource DTO
```

### Update Operation

```
Client Request
    ↓
API Controller (PUT /api/resource/{id})
    ↓
UpdateResourceCommand
    ↓
Validation Behavior
    ↓
Authorization Check (Can update resource?)
    ↓
UpdateResourceCommandHandler
    ├── Load Aggregate from Repository
    ├── Apply Changes (Domain Method)
    ├── Validate Business Rules
    ├── Repository.UpdateAsync()
    ├── UnitOfWork.SaveChangesAsync()
    └── Raise Domain Events
    ↓
Cache Invalidation
    ├── _cache.RemoveAsync($"resource:{id}")
    └── _cache.RemoveAsync($"resources:list")
    ↓
Return 200 OK + Updated Resource DTO
```

### Delete Operation

```
Client Request
    ↓
API Controller (DELETE /api/resource/{id})
    ↓
DeleteResourceCommand
    ↓
Authorization Check (Can delete resource?)
    ↓
DeleteResourceCommandHandler
    ├── Load Aggregate
    ├── Check Business Rules (Can delete?)
    ├── Mark as Deleted or Hard Delete
    ├── Repository.DeleteAsync()
    ├── UnitOfWork.SaveChangesAsync()
    └── Raise Domain Events
    ↓
Cache Invalidation (All related caches)
    ↓
Return 204 No Content
```

---

## 🔄 Caching Strategy Flow

### Cache-Aside Pattern (Read)

```
Query Request
    ↓
Check Cache (Redis)
    ├── Cache Hit
    │   ├── Log: "Cache hit for key: {key}"
    │   └── Return cached data
    │
    └── Cache Miss
        ├── Log: "Cache miss for key: {key}"
        ├── Query Database
        ├── Serialize to JSON
        ├── Store in Cache
        │   ├── Key: domain-specific key
        │   ├── Value: JSON string
        │   ├── Absolute Expiration: 60 minutes
        │   └── Sliding Expiration: 30 minutes
        └── Return data
```

**Implementation Example:**

```csharp
public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery query)
{
    // Cache key
    var cacheKey = $"project:{query.ProjectId}";
    
    // Try get from cache
    var cachedProject = await _cache.GetStringAsync(cacheKey);
    if (!string.IsNullOrEmpty(cachedProject))
    {
        _logger.LogInformation("Cache hit for {Key}", cacheKey);
        var projectDto = JsonSerializer.Deserialize<ProjectDto>(cachedProject);
        return Result<ProjectDto>.Success(projectDto);
    }
    
    // Cache miss - query database
    _logger.LogInformation("Cache miss for {Key}", cacheKey);
    var project = await _context.Projects
        .Include(p => p.Organization)
        .Include(p => p.ProjectManager)
        .FirstOrDefaultAsync(p => p.Id == query.ProjectId);
    
    if (project is null)
        return Result<ProjectDto>.Failure("Project not found");
    
    var result = ProjectDto.FromEntity(project);
    
    // Cache the result
    var options = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };
    
    await _cache.SetStringAsync(
        cacheKey,
        JsonSerializer.Serialize(result),
        options
    );
    
    return Result<ProjectDto>.Success(result);
}
```

### Cache Invalidation Flow

```
Write Operation (Create/Update/Delete)
    ↓
Command Handler Executes
    ↓
Database Transaction Commits
    ↓
Cache Invalidation Strategy:
    ├── Single Item Invalidation
    │   └── _cache.RemoveAsync($"resource:{id}")
    │
    ├── List Invalidation
    │   └── _cache.RemoveAsync($"resources:list:{filter}")
    │
    └── Pattern-Based Invalidation
        └── Remove all keys matching pattern
            └── _cache.RemoveByPatternAsync("resources:*")
```

**Cache Keys Strategy:**

```
User Cache Keys:
├── user:{userId} - Single user
├── users:email:{email} - User by email
├── users:org:{orgId} - Users by organization
└── users:org:{orgId}:role:{role} - Users by org and role

Project Cache Keys:
├── project:{projectId} - Single project
├── projects:org:{orgId} - Projects by organization
├── projects:org:{orgId}:status:{status} - Projects by org and status
└── projects:manager:{managerId} - Projects by manager

Task Cache Keys:
├── task:{taskId} - Single task
├── tasks:project:{projectId} - Tasks by project
├── tasks:user:{userId} - Tasks by assigned user
└── tasks:project:{projectId}:status:{status} - Tasks by project and status
```

---

## 🎉 Domain Event Flow

### Event Dispatching Flow

```
Command Execution
    ↓
Domain Method Called
    ↓
Business Logic Executes
    ↓
Raise Domain Event
    ├── event.AddDomainEvent(new SomeEvent())
    └── Event stored in entity's DomainEvents collection
    ↓
SaveChanges Called (UnitOfWork)
    ├── EF Core SaveChanges
    ├── Database Transaction Commits
    └── Dispatch Domain Events (Interceptor)
    ↓
Event Handlers Execute
    ├── Handler 1 (async)
    ├── Handler 2 (async)
    └── Handler N (async)
    ↓
Clear Domain Events
    └── entity.ClearDomainEvents()
```

**Implementation Details:**

```csharp
// Domain Entity
public class Project : AggregateRoot<ProjectId>
{
    public void Complete()
    {
        if (Status == ProjectStatus.Completed)
            return;
        
        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        // Raise domain event
        AddDomainEvent(new ProjectCompletedEvent(Id));
    }
}

// Domain Event
public record ProjectCompletedEvent(ProjectId ProjectId) : IDomainEvent;

// Event Handler
public class ProjectCompletedEventHandler : INotificationHandler<ProjectCompletedEvent>
{
    public async Task Handle(ProjectCompletedEvent notification, CancellationToken ct)
    {
        // Send notification to project manager
        await _emailService.SendProjectCompletedEmailAsync(notification.ProjectId);
        
        // Update project statistics
        await _statisticsService.UpdateProjectStatisticsAsync(notification.ProjectId);
        
        // Archive completed tasks
        await _taskService.ArchiveCompletedTasksAsync(notification.ProjectId);
        
        _logger.LogInformation(
            "Project {ProjectId} completed and notifications sent",
            notification.ProjectId
        );
    }
}

// EF Core Interceptor
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct)
    {
        await DispatchDomainEventsAsync(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, ct);
    }
    
    private async Task DispatchDomainEventsAsync(DbContext? context)
    {
        if (context is null) return;
        
        var entities = context.ChangeTracker
            .Entries<IEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();
        
        entities.ForEach(e => e.ClearDomainEvents());
        
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }
}
```

---

## 🔄 Complete Request Lifecycle

### Full End-to-End Flow

```
1. CLIENT REQUEST
   ↓
2. API GATEWAY / LOAD BALANCER
   ↓
3. ASP.NET CORE MIDDLEWARE PIPELINE
   ├── Exception Handler Middleware
   ├── CORS Middleware
   ├── Authentication Middleware (JWT)
   ├── Authorization Middleware
   └── Request Logging Middleware
   ↓
4. API CONTROLLER
   ├── Model Binding
   ├── Model Validation (Data Annotations)
   └── Route to Action Method
   ↓
5. MEDIATR (Command/Query Bus)
   ├── Send Command/Query
   └── Pipeline Behaviors
       ├── Logging Behavior (Request/Response)
       ├── Validation Behavior (FluentValidation)
       ├── Performance Behavior (Timing)
       └── Transaction Behavior (UnitOfWork)
   ↓
6. COMMAND/QUERY HANDLER
   ├── Business Logic
   ├── Database Operations
   └── Domain Events
   ↓
7. APPLICATION LAYER
   ├── Use Case Implementation
   ├── Repository Pattern
   └── Domain Service Calls
   ↓
8. DOMAIN LAYER
   ├── Aggregate Operations
   ├── Business Rule Validation
   ├── Domain Event Generation
   └── Value Object Creation
   ↓
9. INFRASTRUCTURE LAYER
   ├── Database Context (EF Core)
   ├── Repository Implementation
   ├── External Service Integration
   └── Cache Service (Redis)
   ↓
10. DATABASE TRANSACTION
    ├── Begin Transaction
    ├── Execute SQL Commands
    ├── Commit Transaction
    └── Dispatch Domain Events
    ↓
11. DOMAIN EVENT HANDLERS
    ├── Send Notifications
    ├── Update Statistics
    ├── Invalidate Cache
    └── Trigger Background Jobs
    ↓
12. RESPONSE MAPPING
    ├── Map Entity to DTO
    ├── Create Result<T> object
    └── Serialize to JSON
    ↓
13. HTTP RESPONSE
    ├── Status Code (200, 201, 400, 404, 500)
    ├── Response Headers
    └── JSON Body
    ↓
14. CLIENT RECEIVES RESPONSE
```

**Example: Create Task Request Lifecycle**

```
POST /api/tasks
Content-Type: application/json
Authorization: Bearer eyJhbGc...

{
  "projectId": "10000000-0000-0000-0000-000000000001",
  "title": "Implement user profile page",
  "description": "Create user profile with edit capabilities",
  "priority": "High",
  "assignedToId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "dueDate": "2025-02-15"
}

↓ Middleware Pipeline ↓

1. Exception Handler Middleware
   └── Wraps entire request in try-catch

2. CORS Middleware
   └── Check Origin header
   └── Add CORS headers

3. Authentication Middleware
   ├── Extract JWT from Authorization header
   ├── Validate token signature
   ├── Validate expiration
   ├── Extract claims (UserId, Role, OrganizationId)
   └── Set HttpContext.User

4. Authorization Middleware
   └── Will check in controller action

↓ Controller ↓

TasksController.CreateTask()
├── [Authorize] - Check if authenticated
├── [RequirePermission("tasks.create")] - Check permission
├── Model Binding - Bind JSON to CreateTaskRequest
├── Model Validation - Validate data annotations
└── Send CreateTaskCommand to Mediator

↓ MediatR Pipeline Behaviors ↓

1. LoggingBehavior<CreateTaskCommand>
   └── Log: "Handling CreateTaskCommand for Project {ProjectId}"

2. ValidationBehavior<CreateTaskCommand>
   ├── CreateTaskCommandValidator.ValidateAsync()
   ├── Check: Title not empty (1-200 chars)
   ├── Check: ProjectId is valid GUID
   ├── Check: DueDate is in future
   └── If invalid → throw ValidationException

3. TransactionBehavior<CreateTaskCommand>
   └── Begin database transaction

4. PerformanceBehavior<CreateTaskCommand>
   └── Start stopwatch

↓ Command Handler ↓

CreateTaskCommandHandler.Handle()
├── Load Project aggregate
│   └── _projectRepository.GetByIdAsync(projectId)
│
├── Verify user has access to project
│   └── _authorizationService.CanAccessProject(userId, projectId)
│
├── Create Task entity
│   ├── TaskId = TaskId.CreateUnique()
│   ├── Task.Create(title, description, priority, dueDate)
│   └── Validate business rules
│       ├── Project must be active
│       ├── Assigned user must be project member
│       └── Due date must be before project end date
│
├── Add task to project
│   └── project.AddTask(task)
│
├── Save to database
│   ├── _taskRepository.AddAsync(task)
│   └── _unitOfWork.SaveChangesAsync()
│       ├── EF Core generates INSERT SQL
│       ├── Execute in transaction
│       └── Commit transaction
│
├── Raise domain events
│   └── TaskCreatedEvent dispatched
│       ├── SendTaskAssignedEmailHandler
│       │   └── Email sent to assigned user
│       ├── UpdateProjectProgressHandler
│       │   └── Recalculate project progress
│       └── InvalidateProjectCacheHandler
│           ├── Remove: "project:{projectId}"
│           └── Remove: "tasks:project:{projectId}"
│
└── Return Result<TaskDto>

↓ Response ↓

HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/tasks/30000000-0000-0000-0000-000000000009

{
  "success": true,
  "data": {
    "id": "30000000-0000-0000-0000-000000000009",
    "projectId": "10000000-0000-0000-0000-000000000001",
    "title": "Implement user profile page",
    "description": "Create user profile with edit capabilities",
    "status": "ToDo",
    "priority": "High",
    "assignedToId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "assignedToName": "Mike Developer",
    "dueDate": "2025-02-15T00:00:00Z",
    "createdAt": "2025-01-30T10:30:00Z"
  },
  "message": "Task created successfully"
}

↓ Logging ↓

[10:30:00 INF] Handled CreateTaskCommand in 145ms
[10:30:00 INF] Task 30000000-0000-0000-0000-000000000009 created for Project 10000000-0000-0000-0000-000000000001
[10:30:00 INF] TaskCreatedEvent dispatched with 3 handlers
[10:30:00 INF] Email sent to mike.dev@acme.com for task assignment
```

---

## ⚠️ Error Handling Flow

### Exception Handling Pipeline

```
Exception Thrown
    ↓
Exception Handler Middleware (Global)
    ↓
Determine Exception Type
    ├── ValidationException → 400 Bad Request
    ├── NotFoundException → 404 Not Found
    ├── UnauthorizedException → 401 Unauthorized
    ├── ForbiddenException → 403 Forbidden
    ├── DomainException → 422 Unprocessable Entity
    └── Unhandled Exception → 500 Internal Server Error
    ↓
Log Exception
    ├── Log Level: Error
    ├── Include Stack Trace
    ├── Include Request Details
    └── Include User Context
    ↓
Create Error Response
    ├── Status Code
    ├── Error Message
    ├── Error Code
    ├── Validation Errors (if applicable)
    └── Correlation ID (for tracking)
    ↓
Return JSON Error Response
```

**Error Response Format:**

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "Title",
        "message": "Title is required"
      },
      {
        "field": "DueDate",
        "message": "Due date must be in the future"
      }
    ],
    "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
  },
  "timestamp": "2025-01-30T10:30:00Z"
}
```

**Implementation:**

```csharp
public class ExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        var (statusCode, error) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred",
                    Details = validationEx.Errors.Select(e => new ValidationError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList(),
                    CorrelationId = correlationId
                }
            ),
            
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = notFoundEx.Message,
                    CorrelationId = correlationId
                }
            ),
            
            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                new ErrorResponse
                {
                    Code = "UNAUTHORIZED",
                    Message = "Authentication required",
                    CorrelationId = correlationId
                }
            ),
            
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ErrorResponse
                {
                    Code = "FORBIDDEN",
                    Message = forbiddenEx.Message,
                    CorrelationId = correlationId
                }
            ),
            
            DomainException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ErrorResponse
                {
                    Code = "DOMAIN_ERROR",
                    Message = domainEx.Message,
                    CorrelationId = correlationId
                }
            ),
            
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred",
                    CorrelationId = correlationId
                }
            )
        };
        
        _logger.LogError(
            exception,
            "Error occurred: {ErrorCode} | CorrelationId: {CorrelationId}",
            error.Code,
            correlationId
        );
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        var result = Result.Failure(error.Message);
        result.Error = error;
        
        await context.Response.WriteAsJsonAsync(result);
    }
}
```

---

## 🔒 Authorization Flow

### Permission-Based Authorization

```
HTTP Request
    ↓
Authentication Middleware
    ├── Validate JWT Token
    ├── Extract Claims (UserId, Role, OrganizationId)
    └── Set HttpContext.User
    ↓
Controller Action with [RequirePermission]
    ↓
Authorization Handler
    ├── Get User from ClaimsPrincipal
    ├── Get Required Permission from Attribute
    ├── Load User Permissions from Database/Cache
    │   └── Cache Key: "permissions:user:{userId}"
    ├── Check if user has required permission
    │   └── Role-based permission mapping
    └── Return Authorized/Unauthorized
    ↓
If Authorized → Execute Action
If Unauthorized → 403 Forbidden
```

**Permission Matrix:**

```
Admin Role:
├── users.* (all user permissions)
├── projects.* (all project permissions)
├── tasks.* (all task permissions)
├── organizations.* (all org permissions)
└── reports.* (all report permissions)

Project Manager Role:
├── projects.read
├── projects.create
├── projects.update (own projects)
├── tasks.* (within managed projects)
├── users.read (within organization)
└── reports.read

Developer Role:
├── projects.read (assigned projects)
├── tasks.read (all)
├── tasks.update (assigned tasks)
├── tasks.create (with approval)
└── timeEntries.*

Viewer Role:
├── projects.read (assigned projects)
├── tasks.read (assigned tasks)
└── reports.read (own reports)
```

**Implementation:**

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;
    
    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        
        if (!HasPermission(role, _permission))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
    
    private bool HasPermission(string role, string permission)
    {
        var rolePermissions = GetRolePermissions(role);
        
        // Check exact permission
        if (rolePermissions.Contains(permission))
            return true;
        
        // Check wildcard permission (e.g., "tasks.*" covers "tasks.create")
        var parts = permission.Split('.');
        var wildcardPermission = $"{parts[0]}.*";
        
        return rolePermissions.Contains(wildcardPermission);
    }
}

// Usage in Controller
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    [HttpPost]
    [RequirePermission("tasks.create")]
    public async Task<IActionResult> CreateTask(CreateTaskRequest request)
    {
        // Only users with "tasks.create" permission can access
    }
    
    [HttpPut("{id}")]
    [RequirePermission("tasks.update")]
    public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
    {
        // Additional resource-based check
        var task = await _mediator.Send(new GetTaskByIdQuery(id));
        
        if (task.AssignedToId != CurrentUserId && !IsAdmin)
        {
            return Forbid(); // Can only update own tasks (unless admin)
        }
        
        // Update task
    }
}
```

---

## 💾 Database Transaction Flow

### UnitOfWork Pattern

```
Command Handler Starts
    ↓
TransactionBehavior (MediatR Pipeline)
    ↓
Begin Transaction
    ├── DbContext.Database.BeginTransactionAsync()
    └── Isolation Level: ReadCommitted
    ↓
Execute Command Handler
    ├── Business Logic
    ├── Repository Operations (tracked by EF)
    │   ├── Add
    │   ├── Update
    │   └── Delete
    └── Domain Event Generation
    ↓
SaveChanges (UnitOfWork)
    ├── EF Core Change Tracker
    ├── Generate SQL Commands
    ├── Execute in Transaction
    └── Handle Concurrency Conflicts
    ↓
Commit Transaction
    ├── All changes committed atomically
    └── Release database locks
    ↓
Dispatch Domain Events
    └── Execute after successful commit
    ↓
If Exception → Rollback Transaction
    ├── Revert all changes
    ├── Log error
    └── Rethrow exception
```

**Implementation:**

```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Commands should use transactions, queries should not
        if (!IsCommand())
            return await next();
        
        // Start transaction
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        
        try
        {
            _logger.LogInformation(
                "Starting transaction for {Request}",
                typeof(TRequest).Name
            );
            
            // Execute command handler
            var response = await next();
            
            // Commit transaction
            await transaction.CommitAsync(ct);
            
            _logger.LogInformation(
                "Transaction committed for {Request}",
                typeof(TRequest).Name
            );
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Transaction rollback for {Request}",
                typeof(TRequest).Name
            );
            
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    private static bool IsCommand()
    {
        return typeof(TRequest).Name.EndsWith("Command");
    }
}

// Usage in Command Handler
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        // Transaction already started by TransactionBehavior
        
        // Create project
        var project = Project.Create(
            request.Name,
            request.Description,
            request.OrganizationId,
            request.ProjectManagerId
        );
        
        // Add to repository (tracked by EF)
        await _projectRepository.AddAsync(project, ct);
        
        // Create initial sprint
        var sprint = Sprint.Create(
            project.Id,
            "Sprint 1",
            "Initial sprint",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(14)
        );
        
        await _sprintRepository.AddAsync(sprint, ct);
        
        // Save all changes in single transaction
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Transaction will be committed by TransactionBehavior
        // Domain events will be dispatched after commit
        
        return Result<ProjectDto>.Success(ProjectDto.FromEntity(project));
    }
}
```

---

## ⚙️ Background Job Flow

### Domain Event → Background Job

```
Domain Event Raised
    ↓
Event Handler
    ↓
Check if async processing needed
    ├── If time-consuming → Queue background job
    └── If quick → Execute immediately
    ↓
Background Job Queue (Hangfire/Quartz)
    ├── Job ID generated
    ├── Job data serialized
    └── Added to queue
    ↓
Background Worker picks up job
    ↓
Execute Job
    ├── Load job data
    ├── Execute task
    ├── Handle errors
    └── Update job status
    ↓
Job Complete
    └── Log completion
```

**Example: Send Email on Project Creation**

```csharp
// Domain Event
public record ProjectCreatedEvent(ProjectId ProjectId) : IDomainEvent;

// Event Handler (Quick notification)
public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    public async Task Handle(ProjectCreatedEvent notification, CancellationToken ct)
    {
        var project = await _projectRepository.GetByIdAsync(notification.ProjectId, ct);
        
        // Quick synchronous tasks
        _logger.LogInformation("Project {ProjectId} created", notification.ProjectId);
        
        // Queue background job for time-consuming tasks
        BackgroundJob.Enqueue<SendProjectCreatedEmailJob>(
            job => job.ExecuteAsync(notification.ProjectId)
        );
        
        // Update statistics (quick)
        await _cache.RemoveAsync($"stats:organization:{project.OrganizationId}", ct);
    }
}

// Background Job
public class SendProjectCreatedEmailJob
{
    public async Task ExecuteAsync(ProjectId projectId)
    {
        _logger.LogInformation("Sending project created email for {ProjectId}", projectId);
        
        var project = await _projectRepository.GetByIdAsync(projectId);
        var projectManager = await _userRepository.GetByIdAsync(project.ProjectManagerId);
        var teamMembers = await _userRepository.GetProjectTeamMembersAsync(projectId);
        
        // Send email to project manager
        await _emailService.SendAsync(new Email
        {
            To = projectManager.Email,
            Subject = $"New Project Created: {project.Name}",
            Body = $"You have been assigned as project manager for {project.Name}"
        });
        
        // Send email to team members
        foreach (var member in teamMembers)
        {
            await _emailService.SendAsync(new Email
            {
                To = member.Email,
                Subject = $"Added to Project: {project.Name}",
                Body = $"You have been added to project {project.Name}"
            });
        }
        
        _logger.LogInformation("Project created emails sent for {ProjectId}", projectId);
    }
}
```

---

## 📊 Performance Considerations

### Query Optimization

```
Query Request
    ↓
1. Check Cache First (Redis)
    └── Return if cached
    ↓
2. Query Database with Optimization
    ├── Use AsNoTracking() for read-only
    ├── Use Select() for projection
    ├── Use Include() for eager loading
    ├── Avoid N+1 queries
    └── Use pagination
    ↓
3. Execute Query
    ├── Generated SQL optimized
    └── Use database indexes
    ↓
4. Cache Result
    └── Store in Redis
    ↓
5. Return Result
```

**Optimized Query Example:**

```csharp
public async Task<Result<PagedList<ProjectDto>>> Handle(
    GetProjectsQuery query,
    CancellationToken ct)
{
    // Check cache for frequently accessed data
    var cacheKey = $"projects:org:{query.OrganizationId}:page:{query.Page}";
    var cached = await _cache.GetAsync<PagedList<ProjectDto>>(cacheKey, ct);
    
    if (cached is not null)
        return Result<PagedList<ProjectDto>>.Success(cached);
    
    // Optimized database query
    var projectsQuery = _context.Projects
        .AsNoTracking() // Read-only, no change tracking
        .Where(p => p.OrganizationId == query.OrganizationId)
        .Include(p => p.ProjectManager) // Eager load to avoid N+1
        .Select(p => new ProjectDto // Project to DTO in database
        {
            Id = p.Id,
            Name = p.Name,
            Status = p.Status.ToString(),
            ProjectManagerName = p.ProjectManager.FirstName + " " + p.ProjectManager.LastName,
            Progress = p.ProgressPercentage
        });
    
    // Apply filters
    if (!string.IsNullOrEmpty(query.SearchTerm))
    {
        projectsQuery = projectsQuery.Where(p =>
            p.Name.Contains(query.SearchTerm) ||
            p.Description.Contains(query.SearchTerm)
        );
    }
    
    // Pagination
    var pagedProjects = await PagedList<ProjectDto>.CreateAsync(
        projectsQuery,
        query.Page,
        query.PageSize,
        ct
    );
    
    // Cache result
    await _cache.SetAsync(cacheKey, pagedProjects, TimeSpan.FromMinutes(5), ct);
    
    return Result<PagedList<ProjectDto>>.Success(pagedProjects);
}
```

---

## 🎯 Summary

This document provides comprehensive flows for all major system operations:

1. **Authentication** - Registration, login, token refresh, logout
2. **CQRS** - Command and query separation with caching
3. **CRUD** - Create, read, update, delete operations
4. **Caching** - Cache-aside pattern with invalidation strategies
5. **Domain Events** - Event-driven architecture
6. **Request Lifecycle** - Complete end-to-end request flow
7. **Error Handling** - Global exception handling with proper responses
8. **Authorization** - Permission-based access control
9. **Transactions** - UnitOfWork pattern with rollback
10. **Background Jobs** - Asynchronous task processing

Each flow includes:
- Visual diagrams
- Implementation details
- Code examples
- Best practices
- Performance considerations

For more details, refer to [ARCHITECTURE.md](ARCHITECTURE.md) and [API-DOCUMENTATION.md](API-DOCUMENTATION.md).
