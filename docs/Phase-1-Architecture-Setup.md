# Phase 1: Solution & Architecture Setup

## ✅ Completed: December 23, 2025

---

## 📋 Overview

This document outlines the complete solution structure, architecture, and initial setup for the **Volcanion Project Management System** Backend API.

---

## 🏗️ Solution Structure

```
volcanion-project-management/
│
├── src/
│   ├── VolcanionPM.Domain/              # Core business logic & entities
│   ├── VolcanionPM.Application/         # Use cases, CQRS, DTOs
│   ├── VolcanionPM.Infrastructure/      # Data access, external services
│   └── VolcanionPM.API/                 # Web API controllers, middleware
│
├── tests/                                # Unit & integration tests
│
├── docs/                                 # Documentation
│
└── VolcanionPM.sln                       # Solution file
```

---

## 🎯 Clean Architecture Layers

### 1. **Domain Layer** (Core)
- **Responsibility**: Business entities, domain logic, interfaces
- **Dependencies**: None (fully independent)
- **Contains**:
  - Entities (Project, Task, User, Sprint, etc.)
  - Value Objects
  - Domain Events
  - Enums
  - Domain Exceptions
  - Repository Interfaces

**Key Principle**: The domain layer knows nothing about the outside world. It's the heart of the application.

---

### 2. **Application Layer**
- **Responsibility**: Use cases, orchestration, business workflows
- **Dependencies**: Domain Layer only
- **Contains**:
  - CQRS Commands & Queries
  - Command Handlers & Query Handlers
  - DTOs (Data Transfer Objects)
  - Mapping Profiles (AutoMapper)
  - Validators (FluentValidation)
  - Pipeline Behaviors (Logging, Validation, Transaction)
  - Application Interfaces

**Key Principle**: This layer defines WHAT the system does, not HOW it does it.

---

### 3. **Infrastructure Layer**
- **Responsibility**: Data persistence, external integrations, technical implementations
- **Dependencies**: Domain & Application Layers
- **Contains**:
  - EF Core DbContexts (WriteDbContext, ReadDbContext)
  - Repository Implementations
  - Unit of Work Implementation
  - Cache Service (Redis)
  - Token Service (JWT)
  - External API clients
  - Database Migrations

**Key Principle**: This layer implements interfaces defined in Application/Domain layers.

---

### 4. **API Layer** (Presentation)
- **Responsibility**: HTTP endpoints, request/response handling
- **Dependencies**: Application & Infrastructure Layers
- **Contains**:
  - Controllers
  - Middleware (Exception Handling, Correlation ID)
  - Filters & Attributes
  - Program.cs (Startup configuration)
  - API Models (if different from DTOs)

**Key Principle**: Thin controllers that delegate to Application layer via MediatR.

---

## 📦 NuGet Packages

### Domain Layer
- None (keeps it pure)

### Application Layer
```xml
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
```

### Infrastructure Layer
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.0" />
```

### API Layer
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
```

---

## 🔧 Program.cs Configuration

The main entry point configures:

1. **Serilog Logging**
   - Structured logging
   - Console and File sinks
   - Log context enrichment

2. **Layer Registration**
   - Application Services (MediatR, Validators, AutoMapper)
   - Infrastructure Services (DbContexts, Repositories, Authentication)

3. **OpenTelemetry**
   - Metrics (ASP.NET Core, HTTP, Runtime)
   - Tracing (ASP.NET Core, HTTP, EF Core)
   - Prometheus exporter

4. **Middleware Pipeline**
   - CorrelationId
   - Exception Handling
   - CORS
   - Authentication & Authorization

5. **Swagger/OpenAPI**
   - API documentation
   - JWT authentication support

---

## 🔐 Security Configuration

### JWT Settings (appsettings.json)
```json
"Jwt": {
  "SecretKey": "your-super-secret-key-change-this-in-production-minimum-32-characters",
  "Issuer": "VolcanionPM.API",
  "Audience": "VolcanionPM.Client",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

### Authentication Flow
- Access Token: 15 minutes (short-lived)
- Refresh Token: 7 days (stored securely)
- Token rotation enabled
- Bearer scheme with JWT

---

## 🗄️ Database Configuration

### PostgreSQL Connections
```json
"ConnectionStrings": {
  "WriteDatabase": "Host=localhost;Port=5432;Database=volcanionpm_write;Username=postgres;Password=your_password",
  "ReadDatabase": "Host=localhost;Port=5432;Database=volcanionpm_read;Username=postgres;Password=your_password"
}
```

**CQRS Pattern**:
- **WriteDbContext**: Handles all write operations (Commands)
- **ReadDbContext**: Handles all read operations (Queries)
- Same PostgreSQL engine, different contexts for optimization

---

## 📊 Observability Configuration

### OpenTelemetry
- **Metrics**: ASP.NET Core, HTTP Client, Runtime
- **Tracing**: Request tracing across layers
- **Prometheus Endpoint**: `/metrics`

### Serilog
- **Console Output**: Development debugging
- **File Output**: Persistent logs (daily rolling)
- **Structured Logging**: JSON format
- **Correlation ID**: Request tracking

---

## 🚀 Middleware

### 1. CorrelationIdMiddleware
- Adds unique ID to each request
- Propagates through all layers
- Included in logs and responses
- Header: `X-Correlation-ID`

### 2. ExceptionHandlingMiddleware
- Global exception catching
- Standardized error responses
- Proper HTTP status codes
- Secure error messages (no stack traces in production)

---

## 🧩 Dependency Injection

### Application Layer Registration
```csharp
services.AddApplicationServices();
```
Registers:
- MediatR handlers
- FluentValidation validators
- AutoMapper profiles
- Pipeline behaviors

### Infrastructure Layer Registration
```csharp
services.AddInfrastructureServices(configuration);
```
Registers:
- DbContexts
- Repositories
- UnitOfWork
- Cache Service
- Token Service
- JWT Authentication

---

## ✅ Design Principles Applied

### SOLID
- **S**: Single Responsibility - Each class has one reason to change
- **O**: Open/Closed - Extensions through interfaces
- **L**: Liskov Substitution - Interfaces properly defined
- **I**: Interface Segregation - Small, focused interfaces
- **D**: Dependency Inversion - Depend on abstractions

### Clean Architecture
- **Independence**: Domain layer has no dependencies
- **Testability**: Business logic isolated and testable
- **Framework Independence**: Can swap EF Core, Redis, etc.
- **Database Independence**: Repository pattern abstracts data access

### CQRS
- **Separation**: Commands change state, Queries read state
- **Optimization**: Separate read/write models
- **Scalability**: Can scale reads and writes independently

---

## 🎯 Next Steps

With Phase 1 complete, we now have:
✅ Solution structure
✅ Project dependencies
✅ Base configuration
✅ Middleware
✅ Logging & Observability setup
✅ Authentication framework

**Phase 2** will implement the Domain Layer with core entities.

---

## 📝 Notes

- All paths are absolute and workspace-relative
- .NET 10 is used (adjust if using stable .NET 9)
- PostgreSQL version 14+ recommended
- Redis 6+ recommended
- This is a production-ready foundation

---

**Generated**: December 23, 2025
**Status**: ✅ Complete
