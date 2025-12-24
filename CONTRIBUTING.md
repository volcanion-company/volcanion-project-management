# Contributing to Volcanion Project Management System

Thank you for your interest in contributing to the Volcanion Project Management System! This document provides guidelines and instructions for contributing to this project.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

---

## Code of Conduct

### Our Standards

- **Be Respectful**: Treat everyone with respect and kindness
- **Be Collaborative**: Work together and help each other
- **Be Professional**: Maintain professionalism in all interactions
- **Be Open-Minded**: Be open to different perspectives and ideas
- **Be Constructive**: Provide constructive feedback and criticism

### Unacceptable Behavior

- Harassment, discrimination, or hate speech
- Trolling, insulting, or derogatory comments
- Personal or political attacks
- Publishing others' private information
- Any conduct that could reasonably be considered inappropriate

---

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker & Docker Compose** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio 2022** or **VS Code** with C# extension
- **PostgreSQL 16** (optional, Docker is recommended)
- **Redis 7** (optional, Docker is recommended)

### Fork and Clone

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/volcanion-project-management.git
   cd volcanion-project-management
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/volcanion-company/volcanion-project-management.git
   ```

---

## Development Setup

### 1. Start Infrastructure Services

Using Docker Compose:

```bash
# Start PostgreSQL and Redis
docker compose up -d postgres redis

# Verify services are running
docker compose ps
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Apply Database Migrations

```bash
dotnet ef database update --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API
```

### 4. Run the Application

```bash
cd src/VolcanionPM.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/scalar/v1`

### 5. Run Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/VolcanionPM.Domain.Tests
dotnet test tests/VolcanionPM.Application.Tests

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Development Workflow

### 1. Create a Feature Branch

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Create a new feature branch
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes

- Write clean, maintainable code following our [coding standards](#coding-standards)
- Add tests for new functionality
- Update documentation as needed
- Ensure all tests pass

### 3. Commit Your Changes

Follow our [commit message guidelines](#commit-message-guidelines):

```bash
git add .
git commit -m "feat: add new feature"
```

### 4. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 5. Create a Pull Request

- Go to the repository on GitHub
- Click "New Pull Request"
- Select your branch
- Fill out the PR template
- Submit the pull request

---

## Coding Standards

### C# Coding Conventions

Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

#### Naming Conventions

```csharp
// Classes, interfaces, methods, properties: PascalCase
public class ProjectService { }
public interface IProjectRepository { }
public string ProjectName { get; set; }
public void CreateProject() { }

// Private fields: _camelCase with underscore prefix
private readonly ILogger _logger;

// Parameters, local variables: camelCase
public void Create(string projectName) {
    var newProject = new Project();
}

// Constants: PascalCase
public const int MaxRetries = 3;
```

#### Code Organization

```csharp
// Order of class members:
public class Example
{
    // 1. Constants
    public const int MaxValue = 100;
    
    // 2. Fields
    private readonly IService _service;
    
    // 3. Constructors
    public Example(IService service) 
    {
        _service = service;
    }
    
    // 4. Properties
    public string Name { get; set; }
    
    // 5. Public methods
    public void DoSomething() { }
    
    // 6. Private methods
    private void Helper() { }
}
```

### Clean Architecture Principles

#### Layer Dependencies

```
API Layer → Application Layer → Domain Layer
                ↓
         Infrastructure Layer → Domain Layer
```

**Rules:**
- Domain layer has NO dependencies on other layers
- Application layer depends ONLY on Domain layer
- Infrastructure layer depends ONLY on Domain layer
- API layer depends on Application and Infrastructure layers

#### CQRS Pattern

**Commands** (Write Operations):
```csharp
// Command
public record CreateProjectCommand(
    string Name,
    string Description,
    Guid OrganizationId
) : IRequest<Result<Guid>>;

// Handler
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        // 2. Create entity
        // 3. Save to repository
        // 4. Return result
    }
}
```

**Queries** (Read Operations):
```csharp
// Query
public record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;

// Handler
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch from read database
        // 2. Map to DTO
        // 3. Return result
    }
}
```

### Domain-Driven Design (DDD)

#### Rich Domain Models

```csharp
// ✅ Good: Rich domain model
public class Project : AggregateRoot
{
    public string Name { get; private set; }
    public ProjectStatus Status { get; private set; }
    
    public Result StartProject()
    {
        if (Status != ProjectStatus.Planning)
            return Result.Failure("Cannot start project that is not in planning");
            
        Status = ProjectStatus.Active;
        AddDomainEvent(new ProjectStartedEvent(Id));
        return Result.Success();
    }
}

// ❌ Bad: Anemic domain model
public class Project
{
    public string Name { get; set; }
    public int Status { get; set; }
}
```

#### Value Objects

```csharp
public class Email : ValueObject
{
    public string Value { get; }
    
    private Email(string value) => Value = value;
    
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>("Email cannot be empty");
            
        if (!IsValidEmail(value))
            return Result.Failure<Email>("Invalid email format");
            
        return Result.Success(new Email(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Error Handling

Use the Result pattern instead of exceptions for business logic errors:

```csharp
// ✅ Good: Result pattern
public Result<Project> CreateProject(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result.Failure<Project>("Name is required");
        
    var project = new Project(name);
    return Result.Success(project);
}

// ❌ Bad: Throwing exceptions for validation
public Project CreateProject(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name is required");
        
    return new Project(name);
}
```

---

## Testing Guidelines

### Test Structure

Follow the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public void StartProject_WhenInPlanning_ShouldSucceed()
{
    // Arrange
    var project = Project.Create("Test Project", OrganizationId.Create()).Value;
    
    // Act
    var result = project.StartProject();
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    project.Status.Should().Be(ProjectStatus.Active);
}
```

### Test Naming

Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`

```csharp
// ✅ Good
[Fact]
public void StartProject_WhenInPlanning_ShouldSucceed() { }

[Fact]
public void StartProject_WhenAlreadyActive_ShouldFail() { }

// ❌ Bad
[Fact]
public void Test1() { }

[Fact]
public void StartProjectTest() { }
```

### Test Coverage

- **Domain Layer**: 100% coverage of business logic
- **Application Layer**: Test all command/query handlers
- **Integration Tests**: Test API endpoints and database interactions
- **Unit Tests**: Test individual components in isolation

### Using FluentAssertions

```csharp
// ✅ Readable assertions
result.IsSuccess.Should().BeTrue();
project.Name.Should().Be("Test Project");
projects.Should().HaveCount(5);
projects.Should().Contain(p => p.Name == "Important Project");

// ❌ Less readable
Assert.True(result.IsSuccess);
Assert.Equal("Test Project", project.Name);
Assert.Equal(5, projects.Count);
```

---

## Commit Message Guidelines

We follow the **Conventional Commits** specification:

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code refactoring without changing functionality
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **build**: Build system or dependency changes
- **ci**: CI/CD configuration changes
- **chore**: Other changes that don't modify src or test files

### Examples

```bash
# Feature
feat(auth): add refresh token rotation

# Bug fix
fix(api): resolve null reference in project controller

# Documentation
docs(readme): update installation instructions

# Refactoring
refactor(domain): extract validation logic to separate class

# Breaking change
feat(api)!: change authentication endpoint structure

BREAKING CHANGE: The login endpoint now returns a different response format
```

### Rules

1. Use imperative, present tense: "add" not "added" or "adds"
2. Don't capitalize first letter
3. No period (.) at the end
4. Keep subject line under 72 characters
5. Use body to explain what and why, not how

---

## Pull Request Process

### Before Submitting

- [ ] All tests pass locally
- [ ] Code follows coding standards
- [ ] New code has appropriate test coverage
- [ ] Documentation is updated if needed
- [ ] Commit messages follow guidelines
- [ ] Branch is up to date with main

### PR Checklist

When creating a pull request, include:

1. **Description**: Clear description of changes
2. **Motivation**: Why is this change needed?
3. **Related Issues**: Link to related issues
4. **Testing**: How was this tested?
5. **Screenshots**: If applicable (for UI changes)
6. **Breaking Changes**: List any breaking changes

### PR Template

```markdown
## Description
Brief description of changes

## Motivation and Context
Why is this change required? What problem does it solve?

## Related Issues
Fixes #123

## How Has This Been Tested?
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Types of Changes
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to change)

## Checklist
- [ ] My code follows the code style of this project
- [ ] I have updated the documentation accordingly
- [ ] I have added tests to cover my changes
- [ ] All new and existing tests passed
```

### Review Process

1. **Automated Checks**: CI/CD pipeline must pass
2. **Code Review**: At least one maintainer approval required
3. **Testing**: Ensure all tests pass
4. **Documentation**: Verify documentation is updated
5. **Merge**: Maintainer will merge after approval

---

## Reporting Bugs

### Before Reporting

- Check if the bug has already been reported
- Try to reproduce the bug with the latest version
- Gather relevant information (logs, screenshots, etc.)

### Bug Report Template

```markdown
## Description
A clear description of the bug

## Steps to Reproduce
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

## Expected Behavior
What you expected to happen

## Actual Behavior
What actually happened

## Environment
- OS: [e.g., Windows 11, Ubuntu 22.04]
- .NET Version: [e.g., 10.0.1]
- Docker Version: [e.g., 24.0.7]

## Logs
```
Paste relevant logs here
```

## Screenshots
If applicable, add screenshots
```

---

## Suggesting Features

### Feature Request Template

```markdown
## Feature Description
Clear description of the proposed feature

## Problem Statement
What problem does this feature solve?

## Proposed Solution
Describe how you envision this feature working

## Alternatives Considered
What other solutions have you considered?

## Additional Context
Any other context or screenshots about the feature
```

---

## Architecture Guidelines

### Adding a New Entity

1. **Domain Layer**:
   - Create entity in `src/VolcanionPM.Domain/Entities/`
   - Define domain events in `src/VolcanionPM.Domain/Events/`
   - Add enumerations if needed

2. **Infrastructure Layer**:
   - Create entity configuration in `src/VolcanionPM.Infrastructure/Persistence/Configurations/`
   - Add repository interface and implementation
   - Create database migration

3. **Application Layer**:
   - Create DTOs in `src/VolcanionPM.Application/DTOs/`
   - Create commands/queries in `src/VolcanionPM.Application/Features/`
   - Add validators using FluentValidation
   - Create AutoMapper profiles

4. **API Layer**:
   - Create controller in `src/VolcanionPM.API/Controllers/`
   - Add API documentation attributes

### Adding a New Feature

1. Create a feature branch
2. Implement domain logic first (TDD approach)
3. Add application layer handlers
4. Create API endpoints
5. Write tests at each layer
6. Update documentation
7. Submit pull request

---

## Questions?

If you have questions:

1. Check existing documentation
2. Search through closed issues
3. Open a new discussion on GitHub
4. Contact maintainers

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to Volcanion Project Management System! 🚀
