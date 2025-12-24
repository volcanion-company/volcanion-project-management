# Volcanion Project Management System

[![CI/CD Pipeline](https://github.com/volcanion-company/volcanion-project-management/workflows/CI%2FCD%20Pipeline/badge.svg)](https://github.com/volcanion-company/volcanion-project-management/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/docker-ready-blue.svg)](https://hub.docker.com/)
[![Tests](https://img.shields.io/badge/tests-286%20passing-success.svg)](https://github.com/volcanion-company/volcanion-project-management)
[![Security](https://img.shields.io/badge/security-OWASP%20compliant-brightgreen.svg)](SECURITY-AUDIT.md)

## 🎯 Overview

Enterprise-grade Project Management System built with **.NET 10**, **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** patterns. This comprehensive backend API provides complete project management capabilities including Agile/Scrum workflows, task tracking, sprint management, time tracking, risk management, document management, and enterprise-level security features.

### ✨ Key Features

#### 🏗️ Architecture & Design
- **Clean Architecture** with 4 distinct layers (Domain, Application, Infrastructure, Presentation)
- **Domain-Driven Design (DDD)** with rich domain models, aggregates, and domain events
- **CQRS Pattern** for command/query separation and optimal read/write performance
- **Event-Driven Architecture** with 25+ domain events
- **Repository & Unit of Work** patterns for data access

#### 🔐 Security & Authentication
- **JWT Authentication** with access tokens (15min) & refresh tokens (7 days)
- **Role-Based Authorization (RBAC)** with fine-grained permissions
- **Account Lockout** (5 failed attempts, 15-minute lockout)
- **Password Policy** (BCrypt hashing, complexity requirements)
- **Rate Limiting** (Auth: 5 req/min, Global: 100 req/min)
- **Security Headers** (CSP, HSTS, X-Frame-Options, X-Content-Type-Options)
- **OWASP Top 10 Compliant** with security audit documentation

#### 📊 Project Management Features
- **Multi-Tenant** organization support
- **Project Management** with budget tracking, progress monitoring, status workflows
- **Agile/Scrum Support** with sprint planning, story points, velocity tracking
- **Task Management** with hierarchical tasks, assignments, blocking, comments
- **Time Tracking** with billable/non-billable hours
- **Risk Management** with probability/impact scoring and mitigation strategies
- **Issue Tracking** with severity levels and resolution workflows
- **Document Management** with version control and file metadata
- **Resource Allocation** with allocation percentage and hourly rates

#### 🚀 Performance & Scalability
- **Redis Caching** with automatic cache invalidation
- **Read/Write Database Separation** for optimized query performance
- **Response Compression** for reduced bandwidth
- **Pagination & Filtering** on all list endpoints
- **Database Query Optimization** with EF Core best practices

#### 📈 Observability & Monitoring
- **Structured Logging** with Serilog and correlation IDs
- **OpenTelemetry Integration** for metrics and distributed tracing
- **Prometheus Metrics** endpoint with custom metrics
- **Health Checks** (liveness, readiness, startup probes)
- **Request/Response Logging** middleware

#### 🐳 DevOps & Deployment
- **Docker Ready** with multi-stage builds (367MB optimized image)
- **Docker Compose** for full-stack local development
- **GitHub Actions CI/CD** with automated testing, security scanning, and deployment
- **Kubernetes Manifests** for production deployment
- **Cloud Deployment Guides** (Azure, AWS, GCP)

#### ✅ Quality & Testing
- **286 Tests Passing** (190 Domain + 96 Application)
- **Unit Tests** with xUnit, FluentAssertions, and Moq
- **FluentValidation** for comprehensive input validation
- **Comprehensive Documentation** (2000+ lines)

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

## 🚀 Quick Start

### Using Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/volcanion-company/volcanion-project-management.git
cd volcanion-project-management

# Start all services (PostgreSQL, Redis, API)
docker compose up -d

# Access the application
# API: http://localhost:8080
# Swagger UI: http://localhost:8080/scalar/v1
# Health: http://localhost:8080/health
# Metrics: http://localhost:8080/metrics

# View API logs
docker compose logs -f api

# Check service status
docker compose ps

# Stop all services
docker compose down
```

### Local Development

```bash
# Prerequisites: .NET 10 SDK, PostgreSQL 16, Redis 7

# Start infrastructure
docker compose up -d postgres redis

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API

# Run the API
cd src/VolcanionPM.API
dotnet run

# Access Swagger at https://localhost:5001/scalar/v1
```

---

## 🚀 Technology Stack

### Core
- **.NET 10** (latest LTS)
- **ASP.NET Core Web API** with minimal APIs
- **C# 13** with nullable reference types

### Architecture Patterns
- **Clean Architecture** - Separation of concerns
- **Domain-Driven Design (DDD)** - Rich domain model
- **CQRS** (Command Query Responsibility Segregation)
- **Mediator Pattern** (MediatR 12.4)
- **Repository + Unit of Work** patterns

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

### Testing
- **xUnit** - Testing framework with 286 tests
- **FluentAssertions** - Readable assertion library
- **Moq** - Mocking framework for dependencies
- **Test Coverage** - 190 Domain + 96 Application tests

### DevOps & Deployment
- **Docker** - Multi-stage containerization
- **Docker Compose** - Local development environment
- **GitHub Actions** - CI/CD pipeline with testing & security scanning
- **Kubernetes** - Production deployment manifests

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
- [x] Solution structure with Clean Architecture
- [x] Project dependencies configured
- [x] Base Program.cs configuration
- [x] Middleware (CorrelationId, Exception Handling, Request Logging)
- [x] Logging setup (Serilog)
- [x] OpenTelemetry configuration
- [x] Swagger/Scalar API documentation
- [x] CORS configuration

### ✅ Phase 2: Domain Layer (COMPLETE)
- [x] Base entities & aggregate roots
- [x] 11 domain entities with rich behavior
- [x] 4 value objects (Email, Money, DateRange, Address)
- [x] 13 domain enumerations
- [x] 25+ domain events
- [x] Business rules & validation
- [x] Rich domain model
- [x] **190 Domain tests passing**

### ✅ Phase 3: Infrastructure Layer (COMPLETE)
- [x] WriteDbContext (EF Core 10)
- [x] ReadDbContext (EF Core 10)
- [x] Repository implementations for all entities
- [x] Unit of Work pattern
- [x] PostgreSQL 16 configuration
- [x] Redis 7 cache service
- [x] 18 Database migrations
- [x] Read/Write database separation

### ✅ Phase 4: Application Layer (COMPLETE)
- [x] 120+ Commands & CommandHandlers
- [x] 50+ Queries & QueryHandlers
- [x] DTOs with comprehensive validation
- [x] MediatR pipeline behaviors
- [x] FluentValidation validators
- [x] AutoMapper profiles
- [x] **96 Application tests passing**

### ✅ Phase 5: Authentication & Authorization (COMPLETE)
- [x] JWT Bearer authentication
- [x] Access & Refresh tokens
- [x] Token service with automatic refresh
- [x] Login/Register endpoints
- [x] Refresh token rotation
- [x] Role-based authorization (RBAC)
- [x] Account lockout (5 failed attempts)
- [x] Password policy enforcement

### ✅ Phase 6: API Layer (COMPLETE)
- [x] 13 Controllers with full CRUD operations
- [x] RESTful endpoints following best practices
- [x] Global exception handling
- [x] Request/Response logging middleware
- [x] Scalar API documentation UI
- [x] Health check endpoints

### ✅ Phase 7: Observability & Logging (COMPLETE)
- [x] OpenTelemetry metrics & tracing
- [x] Prometheus metrics endpoint (/metrics)
- [x] Structured logging with Serilog
- [x] Correlation ID tracking
- [x] Health checks (liveness, readiness, startup)
- [x] Performance counters

### ✅ Phase 8: Caching & Performance (COMPLETE)
- [x] Redis caching strategy
- [x] Cache-aside pattern implementation
- [x] Automatic cache invalidation on updates
- [x] Performance optimization
- [x] Response compression
- [x] Database query optimization

### ✅ Phase 9: Reporting & Dashboard APIs (COMPLETE)
- [x] Aggregated read models
- [x] KPI calculation endpoints
- [x] Dashboard data endpoints
- [x] Project statistics APIs
- [x] Task analytics
- [x] Sprint metrics

### ✅ Phase 10: Security & Testing (COMPLETE)
- [x] OWASP Top 10 compliance audit
- [x] Security headers (CSP, HSTS, X-Frame-Options, etc.)
- [x] Rate limiting (auth: 5/min, global: 100/min)
- [x] Input validation middleware
- [x] SQL injection prevention
- [x] XSS protection mechanisms
- [x] CSRF protection
- [x] **286 Tests passing** (190 Domain + 96 Application)

### ✅ Phase 11: Deployment & DevOps (95% COMPLETE)
- [x] Multi-stage Dockerfile (security hardened)
- [x] Docker Compose for full stack deployment
- [x] GitHub Actions CI/CD pipeline
- [x] Kubernetes deployment manifests
- [x] Cloud deployment guides (Azure, AWS, GCP)
- [x] Comprehensive deployment documentation
- [x] Operations runbook & incident response
- [ ] Final documentation polish (in progress)

---

## 📚 Documentation

**Essential Guides:**
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Complete deployment guide for Docker, Kubernetes, and cloud platforms
- **[OPERATIONS.md](OPERATIONS.md)** - Operations manual, runbook procedures, incident response
- **[SECURITY-AUDIT.md](SECURITY-AUDIT.md)** - Security audit report and OWASP Top 10 compliance
- **[ROADMAP.md](ROADMAP.md)** - Detailed 30-day development roadmap with progress tracking

**API Documentation:**
- **Scalar UI** - Interactive API documentation at `/scalar/v1`
- **OpenAPI Spec** - Download OpenAPI JSON at `/swagger/v1/swagger.json`

---

## 🔧 Configuration

### Environment Variables

Key configuration settings (see [DEPLOYMENT.md](DEPLOYMENT.md) for complete list):

```bash
# Database
ConnectionStrings__WriteDatabase="Host=localhost;Port=5432;Database=volcanionpm;Username=postgres;Password=xxx"
ConnectionStrings__ReadDatabase="Host=localhost;Port=5432;Database=volcanionpm;Username=postgres;Password=xxx"

# Redis Cache
Redis__Configuration="localhost:6379"
Redis__InstanceName="VolcanionPM:"

# JWT Authentication
Jwt__SecretKey="your-super-secret-key-minimum-32-characters-change-in-production"
Jwt__Issuer="VolcanionPM.API"
Jwt__Audience="VolcanionPM.Client"
Jwt__AccessTokenExpirationMinutes="15"
Jwt__RefreshTokenExpirationDays="7"

# Security
Security__Cors__AllowedOrigins__0="http://localhost:3000"
Security__AccountLockout__MaxFailedAttempts="5"
Security__AccountLockout__LockoutDurationMinutes="15"
```

### Docker Compose Configuration

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]
  
  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]
  
  api:
    build: .
    ports: ["8080:8080"]
    depends_on:
      - postgres
      - redis
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/VolcanionPM.Domain.Tests
dotnet test tests/VolcanionPM.Application.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Current Test Coverage:**
- ✅ **286 Tests Passing**
- 190 Domain Layer tests
- 96 Application Layer tests
- Integration tests deferred (EF Core provider conflict)

---

## 🚀 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout (invalidate refresh token)

### Projects
- `GET /api/projects` - List all projects
- `POST /api/projects` - Create new project
- `GET /api/projects/{id}` - Get project details
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Tasks
- `GET /api/tasks` - List all tasks
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}/status` - Update task status
- `POST /api/tasks/{id}/comments` - Add comment

### Dashboard
- `GET /api/dashboard/kpis` - Get dashboard KPIs
- `GET /api/dashboard/project-stats/{projectId}` - Project statistics

### Health & Metrics
- `GET /health` - Combined health check
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe
- `GET /metrics` - Prometheus metrics

For complete API documentation, visit `/scalar/v1` when running the application.

---

## 🔧 Development

### Prerequisites
- .NET 10 SDK
- PostgreSQL 16+
- Redis 7+
- Docker & Docker Compose (optional but recommended)

### Local Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/volcanion-project-management.git
   cd volcanion-project-management
   ```

2. **Start infrastructure services**
   ```bash
   docker compose up -d postgres redis
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project src/VolcanionPM.Infrastructure --startup-project src/VolcanionPM.API
   ```

4. **Run the API**
   ```bash
   cd src/VolcanionPM.API
   dotnet run
   ```

5. **Access the API**
   - API: https://localhost:5001
   - Swagger: https://localhost:5001/scalar/v1
   - Health: https://localhost:5001/health

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

- ✅ **JWT Authentication** - Access & refresh token rotation
- ✅ **Role-Based Authorization (RBAC)** - Fine-grained access control
- ✅ **Account Lockout** - 5 failed attempts, 15-minute lockout
- ✅ **Password Policy** - Minimum 8 characters, complexity requirements
- ✅ **Security Headers** - CSP, HSTS, X-Frame-Options, etc.
- ✅ **Rate Limiting** - Auth: 5 req/min, Global: 100 req/min
- ✅ **Input Validation** - SQL injection & XSS prevention
- ✅ **OWASP Top 10 Compliant** - Security audit complete

See [SECURITY-AUDIT.md](SECURITY-AUDIT.md) for complete security report.

---

## 📊 Observability

### Health Checks
- `/health` - Combined health check (liveness + readiness)
- `/health/live` - Liveness probe (service is running)
- `/health/ready` - Readiness probe (ready to accept traffic)
- `/health/startup` - Startup probe (initialization complete)

### Metrics
- `/metrics` - Prometheus metrics endpoint
- Custom metrics: request duration, cache hit rate, database query time
- OpenTelemetry integration for distributed tracing

### Logging
- Structured logging with Serilog
- Log levels: Information, Warning, Error
- Correlation ID tracking across requests
- Request/response logging middleware

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/VolcanionPM.Domain.Tests
```

**Test Status:**
- ✅ 286 tests passing
- 190 Domain Layer tests (entities, value objects, domain events)
- 96 Application Layer tests (commands, queries, validators)
- Integration tests deferred (EF Core in-memory provider conflict)

---

## 🚀 Deployment

### Quick Deploy with Docker Compose

```bash
docker compose up -d
```

This starts:
- PostgreSQL 16 (port 5432)
- Redis 7 (port 6379)
- API (port 8080)
- pgAdmin (port 5050)

### Kubernetes Deployment

```bash
# Apply all manifests
kubectl apply -f k8s/

# Check deployment status
kubectl get pods -n volcanionpm
kubectl get services -n volcanionpm
```

### Cloud Deployment

Detailed deployment guides for:
- Azure App Service
- AWS ECS Fargate
- Google Cloud Run

See [DEPLOYMENT.md](DEPLOYMENT.md) for complete deployment instructions.

---

## 🔧 Operations

### Common Operations

```bash
# View application logs
docker compose logs -f api

# Check health status
curl http://localhost:8080/health

# View metrics
curl http://localhost:8080/metrics

# Database backup
docker exec -t postgres pg_dump -U postgres volcanionpm > backup.sql

# Database restore
docker exec -i postgres psql -U postgres volcanionpm < backup.sql
```

See [OPERATIONS.md](OPERATIONS.md) for incident response procedures and runbook.

---

## 📝 License

MIT License - See LICENSE file for details

---

## 👥 Contributing

Contributions are welcome! Please read CONTRIBUTING.md for guidelines.

---

## 📞 Support

- 📚 **Documentation:** Comprehensive guides in [DEPLOYMENT.md](DEPLOYMENT.md), [OPERATIONS.md](OPERATIONS.md), and [ARCHITECTURE.md](ARCHITECTURE.md)
- 🐛 **Bug Reports:** [Open an issue](https://github.com/volcanion-company/volcanion-project-management/issues)
- 💡 **Feature Requests:** [Open an issue](https://github.com/volcanion-company/volcanion-project-management/issues)
- 🔒 **Security Issues:** Please report security vulnerabilities responsibly
- 💬 **Questions:** Check documentation or open a discussion

---

## 🎯 Project Stats

- **Lines of Code:** 20,000+
- **Test Coverage:** 286 tests (190 Domain + 96 Application)
- **Controllers:** 13 with full CRUD operations
- **API Endpoints:** 65+
- **Domain Entities:** 11 (Organization, User, Project, Task, Sprint, TimeEntry, Risk, Issue, Document, ResourceAllocation, TaskComment)
- **Value Objects:** 4 (Email, Money, DateRange, Address)
- **Domain Events:** 25+
- **Database Migrations:** 18
- **Commands & Queries:** 170+ (CQRS pattern)
- **Validators:** 60+ (FluentValidation)
- **Docker Image Size:** 367MB (multi-stage optimized)
- **Development Time:** 30 days (full roadmap complete)
- **Documentation:** 2000+ lines across 10+ documents

---

## 📚 Additional Resources

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed architecture documentation
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Deployment guides
- **[OPERATIONS.md](OPERATIONS.md)** - Operations manual
- **[SECURITY-AUDIT.md](SECURITY-AUDIT.md)** - Security audit report
- **[ROADMAP.md](ROADMAP.md)** - Development roadmap
- **[TODO.md](TODO.md)** - Task tracking

---

**Built with ❤️ using .NET 10, Clean Architecture, and Domain-Driven Design**

**Status:** ✅ Production Ready | **Version:** 1.0.0 | **Last Updated:** December 24, 2025

