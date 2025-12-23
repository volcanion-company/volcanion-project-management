# Volcanion Project Management System

## 🎯 Overview

Enterprise-grade Project Management System built with **.NET 10**, **Clean Architecture**, **DDD**, and **CQRS** patterns. This backend API provides comprehensive project management capabilities including task tracking, sprint management, time tracking, risk management, and more.

---

## 🏗️ Architecture

**Clean Architecture** with four distinct layers:

```
┌─────────────────────────────────────┐
│        API Layer (Presentation)     │
│  Controllers, Middleware, Swagger   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     Application Layer (Use Cases)   │
│  CQRS, Handlers, DTOs, Validators   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Infrastructure Layer (Tech)       │
│  EF Core, Repos, Cache, Auth, etc.  │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       Domain Layer (Business)       │
│   Entities, Value Objects, Events   │
└─────────────────────────────────────┘
```

---

## 🚀 Technology Stack

### Core
- **.NET 10** (latest)
- **ASP.NET Core Web API**
- **C# 13** with nullable reference types

### Architecture Patterns
- **Clean Architecture**
- **Domain-Driven Design (DDD)**
- **CQRS** (Command Query Responsibility Segregation)
- **Mediator Pattern** (MediatR)
- **Repository + Unit of Work**

### Data & Persistence
- **PostgreSQL** (Read/Write separation)
- **Entity Framework Core 10**
- **Redis** (Caching)

### Authentication & Security
- **JWT Bearer Authentication**
- **Access + Refresh Tokens**
- **Role-Based Authorization (RBAC)**

### Observability
- **Serilog** (Structured logging)
- **OpenTelemetry** (Metrics & Tracing)
- **Prometheus** (Metrics endpoint)

### Validation & Mapping
- **FluentValidation**
- **AutoMapper**

### Documentation
- **Swagger/OpenAPI 3.0**

---

## 📦 Project Structure

```
volcanion-project-management/
│
├── src/
│   ├── VolcanionPM.Domain/              # Domain entities, value objects, events
│   ├── VolcanionPM.Application/         # CQRS handlers, DTOs, validators
│   ├── VolcanionPM.Infrastructure/      # Data access, external services
│   └── VolcanionPM.API/                 # REST API, controllers, middleware
│
├── tests/                                # Unit & integration tests (coming soon)
│
├── docs/                                 # Comprehensive documentation
│   ├── Phase-1-Architecture-Setup.md
│   └── Phase-2-Domain-Layer.md
│
└── VolcanionPM.sln                       # Solution file
```

---

## ✨ Features

### Core Domains Implemented

#### 🏢 Organization Management
- Multi-tenant support
- Organization profiles
- Subscription management
- User management per organization

#### 👥 User & Role Management
- User authentication (JWT)
- Role-based access control
- Profile management
- Email confirmation
- Password management
- Refresh token support

#### 📊 Project Management
- Project lifecycle (Planning → Active → Completed)
- Budget tracking
- Progress monitoring
- Project manager assignment
- Status workflows
- Priority management

#### ✅ Task Management
- Task creation & assignment
- Hierarchical tasks (parent/subtasks)
- Story point estimation
- Status workflow (Backlog → Done)
- Task blocking/unblocking
- Comments & discussions
- Due date tracking

#### 🏃 Sprint/Agile Support
- Sprint creation & management
- Story point tracking
- Sprint goals
- Velocity calculation
- Sprint completion metrics

#### ⏱️ Time Tracking
- Time entry logging
- Billable/non-billable hours
- Entry type classification
- User time tracking
- Project time aggregation

#### 🎯 Risk Management
- Risk identification
- Probability & impact scoring
- Mitigation strategies
- Risk owner assignment
- Status tracking

#### 🐛 Issue Tracking
- Issue creation & reporting
- Severity levels
- Status workflow
- Assignment tracking
- Resolution management

#### 📄 Document Management
- Document upload & storage
- Version control
- Document type classification
- File metadata tracking

#### 👷 Resource Allocation
- Resource assignment to projects
- Allocation percentage
- Time period tracking
- Hourly rate management

---

## 🎯 Implementation Status

### ✅ Phase 1: Solution & Architecture Setup (COMPLETE)
- [x] Solution structure
- [x] Project dependencies
- [x] Base Program.cs configuration
- [x] Middleware (CorrelationId, Exception Handling)
- [x] Logging setup (Serilog)
- [x] OpenTelemetry configuration
- [x] Swagger/OpenAPI
- [x] CORS configuration

### ✅ Phase 2: Domain Layer (COMPLETE)
- [x] Base entities & aggregate roots
- [x] 11 domain entities
- [x] 4 value objects (Email, Money, DateRange, Address)
- [x] 13 domain enumerations
- [x] 25+ domain events
- [x] Business rules & validation
- [x] Rich domain model

### 🔄 Phase 3: Infrastructure Layer (NEXT)
- [ ] WriteDbContext (EF Core)
- [ ] ReadDbContext (EF Core)
- [ ] Repository implementations
- [ ] Unit of Work
- [ ] PostgreSQL configuration
- [ ] Redis cache service
- [ ] Database migrations

### 📋 Phase 4: Application Layer (UPCOMING)
- [ ] Commands & CommandHandlers
- [ ] Queries & QueryHandlers
- [ ] DTOs
- [ ] MediatR pipeline behaviors
- [ ] Validators
- [ ] AutoMapper profiles

### 🔒 Phase 5: Authentication & Authorization (UPCOMING)
- [ ] JWT configuration
- [ ] Token service
- [ ] Login/Register endpoints
- [ ] Refresh token flow
- [ ] Authorization policies

### 🌐 Phase 6: API Layer (UPCOMING)
- [ ] Controllers
- [ ] RESTful endpoints
- [ ] API models
- [ ] Global exception handling

### 📊 Phase 7: Observability & Logging (UPCOMING)
- [ ] Enhanced OpenTelemetry
- [ ] Prometheus metrics
- [ ] Structured logging patterns
- [ ] Correlation ID tracking

### ⚡ Phase 8: Caching & Performance (UPCOMING)
- [ ] Redis caching strategy
- [ ] Cache-aside pattern
- [ ] Cache invalidation
- [ ] Performance optimization

### 📈 Phase 9: Reporting & Dashboard APIs (UPCOMING)
- [ ] Aggregated read models
- [ ] KPI endpoints
- [ ] Dashboard data

### ✔️ Phase 10: Final Review & Best Practices (UPCOMING)
- [ ] Architecture validation
- [ ] Security checklist
- [ ] Performance review
- [ ] Documentation finalization

---

## 🔧 Configuration

### Database Connection (appsettings.json)
```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=volcanionpm_write;Username=postgres;Password=your_password",
    "ReadDatabase": "Host=localhost;Port=5432;Database=volcanionpm_read;Username=postgres;Password=your_password"
  }
}
```

### Redis Cache
```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "VolcanionPM:"
  }
}
```

### JWT Authentication
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-change-this-in-production",
    "Issuer": "VolcanionPM.API",
    "Audience": "VolcanionPM.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## 🚀 Getting Started (Coming Soon)

```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update --project src/VolcanionPM.Infrastructure

# Run the API
dotnet run --project src/VolcanionPM.API
```

API will be available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`  
Prometheus Metrics: `https://localhost:5001/metrics`

---

## 📚 Documentation

Comprehensive documentation is available in the [docs](./docs) directory:

- [Phase 1: Architecture Setup](./docs/Phase-1-Architecture-Setup.md)
- [Phase 2: Domain Layer](./docs/Phase-2-Domain-Layer.md)
- More phases coming soon...

---

## 🎨 Design Principles

### SOLID Principles
✅ **S**ingle Responsibility  
✅ **O**pen/Closed  
✅ **L**iskov Substitution  
✅ **I**nterface Segregation  
✅ **D**ependency Inversion  

### Clean Architecture
✅ Independence of Frameworks  
✅ Testability  
✅ Independence of UI  
✅ Independence of Database  
✅ Independence of External Services  

### DDD Patterns
✅ Aggregate Roots  
✅ Entities & Value Objects  
✅ Domain Events  
✅ Repository Pattern  
✅ Ubiquitous Language  

---

## 🔐 Security Features

- JWT-based authentication
- Access & Refresh token rotation
- Role-based authorization
- Password hashing
- Email confirmation
- Secure configuration management
- Global exception handling
- Input validation

---

## 📊 Observability

### Logging
- Structured logging with Serilog
- Correlation ID for request tracking
- Console and file outputs
- Log levels and filtering

### Metrics
- ASP.NET Core metrics
- HTTP client metrics
- Runtime metrics
- Prometheus export

### Tracing
- Distributed tracing
- Request flow tracking
- Entity Framework instrumentation

---

## 🧪 Testing (Coming Soon)

- Unit tests
- Integration tests
- API tests
- Performance tests

---

## 📝 License

[Your License Here]

---

## 👥 Contributors

[Your Team Here]

---

## 📞 Support

For questions or issues, please [open an issue](https://github.com/your-org/volcanion-pm/issues).

---

**Built with ❤️ using .NET 10 and Clean Architecture**

**Last Updated**: December 23, 2025
