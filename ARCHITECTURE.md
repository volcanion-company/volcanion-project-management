# Architecture Documentation

## Overview

The Volcanion Project Management System is built using **Clean Architecture** principles combined with **Domain-Driven Design (DDD)** and **CQRS** patterns. This document provides a comprehensive overview of the system architecture, design decisions, and implementation details.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Clean Architecture Layers](#clean-architecture-layers)
- [Domain-Driven Design](#domain-driven-design)
- [CQRS Pattern](#cqrs-pattern)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Data Flow](#data-flow)
- [Security Architecture](#security-architecture)
- [Caching Strategy](#caching-strategy)
- [Database Design](#database-design)
- [API Design](#api-design)
- [Error Handling](#error-handling)
- [Observability](#observability)
- [Design Patterns](#design-patterns)
- [Performance Considerations](#performance-considerations)

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Applications                      │
│                    (Web, Mobile, Desktop)                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS/REST
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                               │
│         Controllers, Middleware, Authentication                  │
│              (Presentation/Infrastructure)                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │ IRequest<T>
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                           │
│        Commands, Queries, Handlers, DTOs, Validators            │
│              (Use Cases/Application Services)                    │
└──────────────┬────────────────────────────┬─────────────────────┘
               │                            │
               │ Domain Models              │ IRepository<T>
               ▼                            ▼
┌──────────────────────────┐  ┌────────────────────────────────────┐
│     Domain Layer         │  │   Infrastructure Layer             │
│  Entities, Value Objects │  │  EF Core, Repositories, Cache,     │
│  Domain Events, Rules    │  │  External Services, Email, etc.    │
│    (Business Logic)      │  │                                    │
└──────────────────────────┘  └────────────┬───────────────────────┘
                                           │
                                           ▼
                              ┌────────────────────────┐
                              │   External Systems     │
                              │  PostgreSQL, Redis     │
                              │  SMTP, etc.            │
                              └────────────────────────┘
```

### Architectural Principles

1. **Dependency Rule**: Dependencies point inward, toward the domain layer
2. **Separation of Concerns**: Each layer has a specific responsibility
3. **Testability**: Business logic is independent of infrastructure
4. **Maintainability**: Clear boundaries make changes easier
5. **Scalability**: Layers can be scaled independently

---

## Clean Architecture Layers

### 1. Domain Layer (Core)

**Purpose**: Contains the business logic and domain models  
**Dependencies**: None (no external dependencies)  
**Location**: `src/VolcanionPM.Domain/`

**Components**:
- **Entities**: Business objects with identity (Project, Task, User, etc.)
- **Value Objects**: Immutable objects without identity (Email, Money, DateRange, Address)
- **Domain Events**: Events that occur in the domain (ProjectCreated, TaskCompleted, etc.)
- **Enumerations**: Domain-specific enumerations (ProjectStatus, TaskStatus, UserRole, etc.)
- **Specifications**: Business rules and specifications
- **Base Classes**: AggregateRoot, Entity, ValueObject

**Key Characteristics**:
- Pure business logic
- No dependencies on frameworks
- No infrastructure concerns
- Fully testable in isolation

**Example Entity**:
```csharp
public class Project : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Money Budget { get; private set; }
    public DateRange Timeline { get; private set; }
    
    // Factory method
    public static Result<Project> Create(
        string name,
        string description,
        OrganizationId organizationId,
        Money budget)
    {
        // Validation logic
        var project = new Project
        {
            Id = ProjectId.Create(),
            Name = name,
            Description = description,
            Status = ProjectStatus.Planning,
            Budget = budget
        };
        
        project.AddDomainEvent(new ProjectCreatedEvent(project.Id));
        return Result.Success(project);
    }
    
    // Domain behavior
    public Result StartProject()
    {
        if (Status != ProjectStatus.Planning)
            return Result.Failure("Cannot start project that is not in planning");
            
        Status = ProjectStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ProjectStartedEvent(Id));
        
        return Result.Success();
    }
}
```

### 2. Application Layer (Use Cases)

**Purpose**: Contains application business rules and use case logic  
**Dependencies**: Domain Layer only  
**Location**: `src/VolcanionPM.Application/`

**Components**:
- **Commands**: Write operations (CreateProject, UpdateTask, etc.)
- **Queries**: Read operations (GetProjectById, GetAllTasks, etc.)
- **Handlers**: Process commands and queries using MediatR
- **DTOs**: Data Transfer Objects for external communication
- **Validators**: FluentValidation validators for input validation
- **Mapping Profiles**: AutoMapper profiles for entity-DTO mapping
- **Pipeline Behaviors**: Cross-cutting concerns (validation, logging, transactions)

**CQRS Implementation**:

**Command Example**:
```csharp
// Command
public record CreateProjectCommand(
    string Name,
    string Description,
    Guid OrganizationId,
    decimal Budget,
    string Currency
) : IRequest<Result<Guid>>;

// Validator
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");
            
        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be non-negative");
    }
}

// Handler
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<Guid>> Handle(
        CreateProjectCommand request, 
        CancellationToken cancellationToken)
    {
        // Create money value object
        var budgetResult = Money.Create(request.Budget, request.Currency);
        if (budgetResult.IsFailure)
            return Result.Failure<Guid>(budgetResult.Error);
            
        // Create project
        var projectResult = Project.Create(
            request.Name,
            request.Description,
            OrganizationId.Create(request.OrganizationId),
            budgetResult.Value);
            
        if (projectResult.IsFailure)
            return Result.Failure<Guid>(projectResult.Error);
            
        // Save to repository
        await _unitOfWork.Projects.AddAsync(projectResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(projectResult.Value.Id.Value);
    }
}
```

**Query Example**:
```csharp
// Query
public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;

// Handler
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IReadDbContext _readDb;
    
    public GetProjectByIdQueryHandler(IReadDbContext readDb)
    {
        _readDb = readDb;
    }
    
    public async Task<Result<ProjectDto>> Handle(
        GetProjectByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var project = await _readDb.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status.ToString(),
                Budget = p.Budget,
                Currency = p.Currency
            })
            .FirstOrDefaultAsync(cancellationToken);
            
        if (project == null)
            return Result.Failure<ProjectDto>("Project not found");
            
        return Result.Success(project);
    }
}
```

### 3. Infrastructure Layer (Technical Implementation)

**Purpose**: Implements technical capabilities (persistence, external services)  
**Dependencies**: Domain Layer only  
**Location**: `src/VolcanionPM.Infrastructure/`

**Components**:
- **Persistence**: EF Core DbContext, configurations, migrations
- **Repositories**: Concrete implementations of repository interfaces
- **Cache**: Redis caching implementation
- **Authentication**: JWT token service, password hashing
- **External Services**: Email service, file storage, etc.
- **Dependency Injection**: Service registration

**Database Context**:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    // ... other DbSets
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit tracking
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        // Publish domain events
        var events = ChangeTracker.Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
            
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Dispatch events after successful save
        foreach (var @event in events)
        {
            await _mediator.Publish(@event, cancellationToken);
        }
        
        return result;
    }
}
```

**Repository Implementation**:
```csharp
public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Project?> GetByIdWithTasksAsync(
        ProjectId id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == ProjectStatus.Active)
            .ToListAsync(cancellationToken);
    }
}
```

### 4. API Layer (Presentation)

**Purpose**: Exposes functionality through HTTP endpoints  
**Dependencies**: Application Layer, Infrastructure Layer  
**Location**: `src/VolcanionPM.API/`

**Components**:
- **Controllers**: REST API endpoints
- **Middleware**: Authentication, error handling, logging, CORS
- **Filters**: Authorization, validation
- **Configuration**: Startup, dependency injection
- **API Documentation**: Swagger/OpenAPI

**Controller Example**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
            return BadRequest(new ProblemDetails { Detail = result.Error });
            
        return CreatedAtAction(
            nameof(GetProjectById), 
            new { id = result.Value }, 
            result.Value);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
            return NotFound(new ProblemDetails { Detail = result.Error });
            
        return Ok(result.Value);
    }
}
```

---

## Domain-Driven Design

### Bounded Contexts

The system is organized into the following bounded contexts:

1. **Organization Management**: Organizations, subscriptions
2. **User Management**: Users, roles, authentication
3. **Project Management**: Projects, project lifecycle
4. **Task Management**: Tasks, task hierarchy, assignments
5. **Sprint Management**: Sprints, agile workflows
6. **Time Tracking**: Time entries, billable hours
7. **Risk Management**: Risks, mitigation strategies
8. **Issue Tracking**: Issues, resolution workflows
9. **Document Management**: Documents, file storage
10. **Resource Allocation**: Resource assignments, availability

### Aggregates

**Aggregate Roots**:
- **Organization**: Root for organization data
- **User**: Root for user profile and authentication
- **Project**: Root for project data and related tasks
- **Sprint**: Root for sprint data and task assignments
- **Risk**: Root for risk management
- **Issue**: Root for issue tracking

**Aggregate Example**:
```csharp
public class Project : AggregateRoot
{
    // Identity
    public ProjectId Id { get; private set; }
    
    // Properties
    public string Name { get; private set; }
    public ProjectStatus Status { get; private set; }
    
    // Value Objects
    public Money Budget { get; private set; }
    public DateRange Timeline { get; private set; }
    
    // References to other aggregates (by ID only)
    public OrganizationId OrganizationId { get; private set; }
    public UserId ProjectManagerId { get; private set; }
    
    // Collection of entities within aggregate
    private readonly List<ProjectTask> _tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks.AsReadOnly();
    
    // Domain behavior
    public Result AddTask(string name, string description)
    {
        var task = ProjectTask.Create(name, description, Id);
        _tasks.Add(task.Value);
        AddDomainEvent(new TaskAddedToProjectEvent(Id, task.Value.Id));
        return Result.Success();
    }
}
```

### Value Objects

Value objects are immutable and defined by their attributes:

```csharp
public class Email : ValueObject
{
    public string Value { get; }
    
    private Email(string value) => Value = value;
    
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>("Email cannot be empty");
            
        if (!IsValidFormat(value))
            return Result.Failure<Email>("Invalid email format");
            
        return Result.Success(new Email(value.ToLowerInvariant()));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    private static bool IsValidFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative");
            
        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency is required");
            
        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }
}
```

### Domain Events

Domain events capture important business occurrences:

```csharp
public record ProjectCreatedEvent(ProjectId ProjectId) : IDomainEvent;

public record ProjectStartedEvent(ProjectId ProjectId) : IDomainEvent;

public record TaskCompletedEvent(ProjectTaskId TaskId, ProjectId ProjectId) : IDomainEvent;

// Event handler
public class ProjectStartedEventHandler : INotificationHandler<ProjectStartedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    
    public async Task Handle(ProjectStartedEvent notification, CancellationToken cancellationToken)
    {
        // Send notification to project manager
        // Update project statistics
        // Log event
    }
}
```

---

## CQRS Pattern

### Command Query Responsibility Segregation

**Separation of Concerns**:
- **Commands**: Modify state (Create, Update, Delete)
- **Queries**: Read state (Get, List, Search)

### Write Model (Commands)

Uses the write database with full EF Core tracking:

```csharp
// Command writes to ApplicationDbContext
public class CreateProjectCommandHandler
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Result<Guid>> Handle(CreateProjectCommand request)
    {
        var project = Project.Create(...);
        _context.Projects.Add(project.Value);
        await _context.SaveChangesAsync(); // Full tracking, domain events
        return Result.Success(project.Value.Id);
    }
}
```

### Read Model (Queries)

Uses the read database with no tracking for performance:

```csharp
// Query reads from ReadDbContext
public class GetProjectsQueryHandler
{
    private readonly ReadDbContext _readDb;
    
    public async Task<Result<List<ProjectDto>>> Handle(GetProjectsQuery request)
    {
        var projects = await _readDb.Projects
            .AsNoTracking() // No change tracking overhead
            .Where(p => p.OrganizationId == request.OrganizationId)
            .Select(p => new ProjectDto { ... }) // Project to DTO
            .ToListAsync();
            
        return Result.Success(projects);
    }
}
```

### Benefits

1. **Performance**: Optimized read and write paths
2. **Scalability**: Read and write databases can be scaled independently
3. **Separation**: Clear distinction between reads and writes
4. **Flexibility**: Different models for different use cases

---

## Technology Stack

### Core Framework
- **.NET 10** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **C# 13** - Programming language

### Data Access
- **Entity Framework Core 10** - ORM
- **PostgreSQL 16** - Relational database
- **Npgsql** - PostgreSQL provider
- **Redis 7** - Caching and distributed cache

### Libraries & Packages
- **MediatR 12.4** - CQRS implementation
- **FluentValidation 11.9** - Input validation
- **AutoMapper 13.0** - Object-object mapping
- **Serilog 8.0** - Structured logging
- **BCrypt.Net-Next** - Password hashing

### Security
- **JWT Bearer Authentication** - Token-based auth
- **ASP.NET Core Identity** - User management
- **Microsoft.AspNetCore.RateLimiting** - Rate limiting

### Testing
- **xUnit** - Unit testing framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **Testcontainers** - Integration testing

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD pipelines
- **Kubernetes** - Container orchestration

### Observability
- **OpenTelemetry** - Metrics and tracing
- **Prometheus** - Metrics collection
- **Serilog** - Structured logging

---

## Project Structure

```
volcanion-project-management/
│
├── src/
│   ├── VolcanionPM.Domain/                    # Domain Layer (Core)
│   │   ├── Entities/                          # Domain entities
│   │   │   ├── Organization.cs
│   │   │   ├── User.cs
│   │   │   ├── Project.cs
│   │   │   ├── ProjectTask.cs
│   │   │   ├── Sprint.cs
│   │   │   ├── TimeEntry.cs
│   │   │   ├── Risk.cs
│   │   │   ├── Issue.cs
│   │   │   ├── Document.cs
│   │   │   ├── ResourceAllocation.cs
│   │   │   └── TaskComment.cs
│   │   ├── ValueObjects/                      # Value objects
│   │   │   ├── Email.cs
│   │   │   ├── Money.cs
│   │   │   ├── DateRange.cs
│   │   │   └── Address.cs
│   │   ├── Events/                            # Domain events
│   │   │   ├── ProjectCreatedEvent.cs
│   │   │   ├── TaskCompletedEvent.cs
│   │   │   └── ... (25+ domain events)
│   │   ├── Enums/                             # Domain enumerations
│   │   │   ├── ProjectStatus.cs
│   │   │   ├── TaskStatus.cs
│   │   │   ├── UserRole.cs
│   │   │   └── ... (13 enumerations)
│   │   ├── Common/                            # Base classes
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── Entity.cs
│   │   │   ├── ValueObject.cs
│   │   │   └── Result.cs
│   │   └── VolcanionPM.Domain.csproj
│   │
│   ├── VolcanionPM.Application/               # Application Layer
│   │   ├── Features/                          # CQRS features
│   │   │   ├── Projects/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProject/
│   │   │   │   │   │   ├── CreateProjectCommand.cs
│   │   │   │   │   │   ├── CreateProjectCommandHandler.cs
│   │   │   │   │   │   └── CreateProjectCommandValidator.cs
│   │   │   │   │   ├── UpdateProject/
│   │   │   │   │   └── DeleteProject/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetProjectById/
│   │   │   │       ├── GetAllProjects/
│   │   │   │       └── GetProjectsByOrganization/
│   │   │   ├── Tasks/
│   │   │   ├── Users/
│   │   │   ├── Organizations/
│   │   │   ├── Sprints/
│   │   │   ├── TimeEntries/
│   │   │   ├── Risks/
│   │   │   ├── Issues/
│   │   │   └── Documents/
│   │   ├── DTOs/                              # Data transfer objects
│   │   ├── Mapping/                           # AutoMapper profiles
│   │   ├── Behaviors/                         # MediatR pipeline behaviors
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── PerformanceBehavior.cs
│   │   │   └── TransactionBehavior.cs
│   │   ├── Interfaces/                        # Application interfaces
│   │   ├── DependencyInjection.cs
│   │   └── VolcanionPM.Application.csproj
│   │
│   ├── VolcanionPM.Infrastructure/            # Infrastructure Layer
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs        # Write database
│   │   │   ├── ReadDbContext.cs               # Read database
│   │   │   ├── Configurations/                # EF Core configurations
│   │   │   │   ├── OrganizationConfiguration.cs
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   └── ... (11 configurations)
│   │   │   └── Migrations/                    # Database migrations
│   │   ├── Repositories/
│   │   │   ├── Repository.cs                  # Generic repository
│   │   │   ├── ProjectRepository.cs
│   │   │   ├── TaskRepository.cs
│   │   │   └── ... (11 repositories)
│   │   ├── Services/
│   │   │   ├── RedisCacheService.cs
│   │   │   ├── TokenService.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   └── EmailService.cs
│   │   ├── UnitOfWork.cs
│   │   ├── DependencyInjection.cs
│   │   └── VolcanionPM.Infrastructure.csproj
│   │
│   └── VolcanionPM.API/                       # API/Presentation Layer
│       ├── Controllers/                       # REST controllers
│       │   ├── AuthController.cs
│       │   ├── ProjectsController.cs
│       │   ├── TasksController.cs
│       │   ├── UsersController.cs
│       │   └── ... (13 controllers)
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       ├── Filters/
│       ├── Extensions/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Dockerfile
│       └── VolcanionPM.API.csproj
│
├── tests/
│   ├── VolcanionPM.Domain.Tests/              # Domain unit tests (190 tests)
│   ├── VolcanionPM.Application.Tests/         # Application unit tests (96 tests)
│   └── VolcanionPM.IntegrationTests/          # Integration tests (deferred)
│
├── docs/                                       # Documentation
├── k8s/                                        # Kubernetes manifests
├── .github/workflows/                          # CI/CD pipelines
├── docker-compose.yml                          # Docker Compose configuration
├── .gitignore
├── README.md
├── ARCHITECTURE.md
├── CONTRIBUTING.md
├── LICENSE
└── VolcanionPM.sln                            # Solution file
```

---

## Data Flow

### Command Flow (Write Operation)

```
Client Request
    │
    ▼
[API Controller]
    │ CreateProjectCommand
    ▼
[MediatR Pipeline]
    │
    ├─► [ValidationBehavior] ──► Validate with FluentValidation
    │
    ├─► [LoggingBehavior] ──► Log request/response
    │
    ├─► [PerformanceBehavior] ──► Measure execution time
    │
    └─► [TransactionBehavior] ──► Wrap in transaction
        │
        ▼
[Command Handler]
    │
    ├─► Create domain entity
    ├─► Validate business rules
    ├─► Raise domain events
    │
    ▼
[Repository]
    │
    ▼
[ApplicationDbContext]
    │
    ├─► Save changes
    ├─► Dispatch domain events
    └─► Commit transaction
        │
        ▼
[PostgreSQL Database]
    │
    ▼
[Cache Invalidation]
    │
    ▼
Response to Client
```

### Query Flow (Read Operation)

```
Client Request
    │
    ▼
[API Controller]
    │ GetProjectByIdQuery
    ▼
[MediatR Pipeline]
    │
    ├─► [LoggingBehavior]
    │
    └─► [PerformanceBehavior]
        │
        ▼
[Query Handler]
    │
    ├─► Check Redis cache
    │   │
    │   ├─► Cache HIT ──► Return cached data
    │   │
    │   └─► Cache MISS ──► Continue to database
    │
    ▼
[ReadDbContext]
    │
    ├─► AsNoTracking() for performance
    ├─► Project to DTO
    └─► Execute query
        │
        ▼
[PostgreSQL Database (Read Replica)]
    │
    ▼
[Cache Result]
    │
    ▼
Response to Client
```

---

## Security Architecture

### Authentication Flow

```
1. User Login
   └─► POST /api/auth/login
       └─► Validate credentials
           └─► Hash password check (BCrypt)
               └─► Generate JWT tokens
                   ├─► Access Token (15 min)
                   └─► Refresh Token (7 days)
                       └─► Store refresh token in database
                           └─► Return tokens to client

2. Authenticated Request
   └─► Client sends Access Token in Authorization header
       └─► JWT Middleware validates token
           ├─► Signature verification
           ├─► Expiration check
           └─► Claims extraction
               └─► Set HttpContext.User
                   └─► Proceed to controller

3. Token Refresh
   └─► POST /api/auth/refresh
       └─► Validate refresh token
           └─► Generate new access token
               └─► Rotate refresh token (optional)
                   └─► Return new tokens
```

### Authorization

**Role-Based Access Control (RBAC)**:

```csharp
// Controller level
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase { }

// Action level
[Authorize(Roles = "ProjectManager,Admin")]
[HttpPost]
public async Task<IActionResult> CreateProject(...) { }

// Custom policy
[Authorize(Policy = "RequireProjectOwner")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProject(Guid id) { }
```

### Security Features

1. **Password Security**:
   - BCrypt hashing with work factor 12
   - Password complexity requirements
   - Minimum 8 characters

2. **Account Lockout**:
   - 5 failed login attempts
   - 15-minute lockout duration
   - Automatic unlock after timeout

3. **Rate Limiting**:
   - Authentication endpoints: 5 requests/minute
   - Global endpoints: 100 requests/minute
   - IP-based tracking

4. **Security Headers**:
   - Content-Security-Policy
   - X-Content-Type-Options: nosniff
   - X-Frame-Options: DENY
   - X-XSS-Protection: 1; mode=block
   - Strict-Transport-Security (HSTS)

5. **CORS Configuration**:
   - Whitelist of allowed origins
   - Credentials support
   - Specific HTTP methods

---

## Caching Strategy

### Cache-Aside Pattern

```csharp
public async Task<Result<ProjectDto>> GetProjectByIdAsync(Guid id)
{
    // 1. Check cache
    var cacheKey = $"project:{id}";
    var cached = await _cache.GetAsync<ProjectDto>(cacheKey);
    
    if (cached != null)
        return Result.Success(cached); // Cache hit
    
    // 2. Cache miss - query database
    var project = await _readDb.Projects
        .AsNoTracking()
        .Where(p => p.Id == id)
        .ProjectToDto()
        .FirstOrDefaultAsync();
    
    if (project == null)
        return Result.Failure<ProjectDto>("Project not found");
    
    // 3. Store in cache
    await _cache.SetAsync(cacheKey, project, TimeSpan.FromMinutes(15));
    
    return Result.Success(project);
}
```

### Cache Invalidation

```csharp
public async Task<Result> UpdateProjectAsync(UpdateProjectCommand command)
{
    // Update in database
    var project = await _unitOfWork.Projects.GetByIdAsync(command.Id);
    project.Update(command.Name, command.Description);
    await _unitOfWork.SaveChangesAsync();
    
    // Invalidate cache
    await _cache.RemoveAsync($"project:{command.Id}");
    await _cache.RemoveAsync($"projects:organization:{project.OrganizationId}");
    
    return Result.Success();
}
```

### Caching Patterns

1. **Entity Cache**: Individual entities (15-minute TTL)
2. **List Cache**: Collection queries (5-minute TTL)
3. **Computed Cache**: Aggregated data (30-minute TTL)
4. **Session Cache**: User-specific data (session lifetime)

---

## Database Design

### Read/Write Separation

**Write Database** (ApplicationDbContext):
- Full EF Core change tracking
- Domain event dispatching
- Transaction support
- Optimistic concurrency

**Read Database** (ReadDbContext):
- No change tracking (`AsNoTracking()`)
- Optimized for queries
- Direct DTO projection
- Read replicas support

### Entity Configuration

```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Table
        builder.ToTable("projects");
        
        // Primary Key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProjectId.Create(value))
            .ValueGeneratedNever();
        
        // Properties
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        // Value Object - Money
        builder.OwnsOne(p => p.Budget, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("budget_amount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("budget_currency")
                .HasMaxLength(3);
        });
        
        // Value Object - DateRange
        builder.OwnsOne(p => p.Timeline, timeline =>
        {
            timeline.Property(t => t.Start)
                .HasColumnName("start_date");
            timeline.Property(t => t.End)
                .HasColumnName("end_date");
        });
        
        // Foreign Key
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(p => p.OrganizationId);
        builder.HasIndex(p => p.Status);
        
        // Audit
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        builder.Property(p => p.UpdatedAt);
    }
}
```

### Migration Strategy

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API

# Update database
dotnet ef database update --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API

# Rollback
dotnet ef database update PreviousMigration --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API

# Generate SQL script
dotnet ef migrations script --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API --output migration.sql
```

---

## API Design

### REST Conventions

- **GET**: Retrieve resources
- **POST**: Create resources
- **PUT**: Update resources (full)
- **PATCH**: Update resources (partial)
- **DELETE**: Delete resources

### Endpoint Patterns

```
GET    /api/projects                  # List all projects
POST   /api/projects                  # Create project
GET    /api/projects/{id}             # Get project by ID
PUT    /api/projects/{id}             # Update project
DELETE /api/projects/{id}             # Delete project

GET    /api/projects/{id}/tasks       # Get tasks for project
POST   /api/projects/{id}/tasks       # Add task to project
```

### Response Formats

**Success Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Project Alpha",
  "description": "Description here",
  "status": "Active",
  "budget": {
    "amount": 100000,
    "currency": "USD"
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required"],
    "Budget": ["Budget must be greater than 0"]
  }
}
```

**Paginated Response** (200 OK):
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5,
  "totalCount": 100,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## Error Handling

### Result Pattern

Instead of throwing exceptions for expected errors:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    private Result(bool isSuccess, T value, string error) 
        : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value) 
        => new Result<T>(true, value, string.Empty);
        
    public static Result<T> Failure(string error) 
        => new Result<T>(false, default!, error);
}
```

### Global Exception Handler

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedException(context, ex);
        }
    }
    
    private async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var problemDetails = new ValidationProblemDetails(
            ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()));
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

---

## Observability

### Structured Logging

```csharp
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "VolcanionPM")
    .WriteTo.Console()
    .WriteTo.File("logs/volcanionpm-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Usage
_logger.LogInformation(
    "Creating project {ProjectName} for organization {OrganizationId}",
    command.Name,
    command.OrganizationId);
```

### Health Checks

```csharp
// Startup configuration
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisConnection, name: "redis")
    .AddDbContextCheck<ApplicationDbContext>(name: "database");

// Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Just checks if app is running
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Metrics

```csharp
// OpenTelemetry configuration
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

// Prometheus endpoint
app.MapPrometheusScrapingEndpoint("/metrics");
```

---

## Design Patterns

### Patterns Used

1. **Repository Pattern**: Data access abstraction
2. **Unit of Work Pattern**: Transaction management
3. **Mediator Pattern**: Request/response handling (MediatR)
4. **Factory Pattern**: Entity creation
5. **Strategy Pattern**: Different caching strategies
6. **Observer Pattern**: Domain events
7. **Decorator Pattern**: MediatR pipeline behaviors
8. **Result Pattern**: Error handling without exceptions
9. **Specification Pattern**: Business rule encapsulation
10. **CQRS Pattern**: Command/query separation

---

## Performance Considerations

### Database Optimization

1. **Indexes**: Strategic indexing on frequently queried columns
2. **AsNoTracking()**: No change tracking for read operations
3. **Pagination**: Limit result sets
4. **Projection**: Select only needed columns
5. **Eager Loading**: `Include()` to avoid N+1 queries
6. **Compiled Queries**: Cached query plans

### Caching Strategy

1. **Redis**: Distributed caching for horizontal scaling
2. **Cache Invalidation**: Remove stale data on updates
3. **TTL**: Appropriate Time-To-Live for different data types
4. **Cache Keys**: Structured naming for easy invalidation

### API Performance

1. **Response Compression**: Gzip compression for responses
2. **Async/Await**: Non-blocking I/O operations
3. **Connection Pooling**: Reuse database connections
4. **Rate Limiting**: Prevent abuse and ensure fair usage

---

## Deployment Architecture

### Container Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Load Balancer                         │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
    ┌────────┐     ┌────────┐     ┌────────┐
    │ API    │     │ API    │     │ API    │
    │ Pod 1  │     │ Pod 2  │     │ Pod 3  │
    └────┬───┘     └────┬───┘     └────┬───┘
         │              │              │
         └──────────────┼──────────────┘
                        │
         ┌──────────────┼──────────────┐
         │              │              │
         ▼              ▼              ▼
    ┌────────┐     ┌────────┐     ┌────────┐
    │ PostgreSQL   │ Redis   │   │ Monitoring │
    │ Primary      │ Cluster │   │ Stack      │
    └────┬───┘     └─────────┘   └────────────┘
         │
         ▼
    ┌────────┐
    │ PostgreSQL │
    │ Replica    │
    └────────────┘
```

---

## Conclusion

The Volcanion Project Management System demonstrates enterprise-level architecture combining Clean Architecture, Domain-Driven Design, and CQRS patterns. The system is designed for:

- **Maintainability**: Clear separation of concerns
- **Testability**: Business logic independent of infrastructure
- **Scalability**: Horizontal scaling capabilities
- **Performance**: Optimized read/write paths with caching
- **Security**: Comprehensive security features
- **Observability**: Full logging, metrics, and tracing

For more information, see:
- [README.md](README.md) - Project overview
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment instructions
- [OPERATIONS.md](OPERATIONS.md) - Operations manual
