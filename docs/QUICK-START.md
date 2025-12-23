# ⚡ QUICK REFERENCE - NEXT ACTIONS
**Last Updated**: December 23, 2025

## 🎯 CURRENT STATUS
- **Phase 3**: ✅ Complete
- **Phase 4**: 🔄 40% Complete
- **Phase 5**: 🔄 30% Complete
- **Phase 6**: 🔄 25% Complete
- **Overall**: 34% Complete

## 🔥 TOP 3 PRIORITIES (DO THESE FIRST)

### 1️⃣ CRITICAL: Fix Password Hashing (2-3 hours)
**Impact**: Security vulnerability  
**Files to Create/Modify**:
```
src/VolcanionPM.Application/Common/Interfaces/IPasswordHasher.cs
src/VolcanionPM.Infrastructure/Services/PasswordHasher.cs
src/VolcanionPM.Infrastructure/DependencyInjection.cs
src/VolcanionPM.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs
src/VolcanionPM.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
```

**Steps**:
1. Install BCrypt: `dotnet add src/VolcanionPM.Infrastructure package BCrypt.Net-Next`
2. Create IPasswordHasher interface
3. Implement PasswordHasher with BCrypt
4. Update RegisterCommandHandler to use IPasswordHasher
5. Update LoginCommandHandler to verify passwords
6. Test: Register → Check DB (should see hash) → Login (should work)

### 2️⃣ BLOCKING: Recreate Entity Configurations (4-6 hours)
**Impact**: Cannot create database migrations  
**Files to Create**: 11 configuration files in `src/VolcanionPM.Infrastructure/Persistence/Configurations/`

**Steps**:
1. Create OrganizationConfiguration.cs
2. Create UserConfiguration.cs
3. Create ProjectConfiguration.cs
4. Create ProjectTaskConfiguration.cs
5. Create SprintConfiguration.cs
6. Create TimeEntryConfiguration.cs
7. Create RiskConfiguration.cs
8. Create IssueConfiguration.cs
9. Create DocumentConfiguration.cs
10. Create ResourceAllocationConfiguration.cs
11. Create TaskCommentConfiguration.cs
12. Test: `dotnet ef migrations add InitialCreate`

### 3️⃣ BLOCKING: Add Domain Update Methods (6-8 hours)
**Impact**: Update operations don't work  
**Files to Modify**: 11 entity files in `src/VolcanionPM.Domain/Entities/`

**Steps**:
1. Add Update methods to Project.cs
2. Add Update methods to ProjectTask.cs
3. Add Update methods to User.cs
4. Add Update methods to Organization.cs
5. Add Update methods to Sprint.cs
6. Add Update methods to remaining entities
7. Test: Call update commands → Verify data changes in DB

---

## 📚 DOCUMENTATION FILES

### Read These First
1. **CRITICAL-ISSUES.md** - 5 blocking issues with solutions
2. **TODO.md** - Complete task list with progress tracking
3. **ROADMAP.md** - 30-day implementation plan

### Reference Documents
4. **docs/PROJECT-ROADMAP.md** - Original 10-phase plan (updated)
5. **docs/PHASE-3-6-COMPLETION-SUMMARY.md** - What was completed today
6. **docs/ENTITY-REFERENCE.md** - Domain entity documentation
7. **README.md** - Project overview

---

## 🚀 QUICK COMMANDS

### Build & Run
```bash
# Build solution
dotnet build

# Run API (after fixing issues)
dotnet run --project src/VolcanionPM.API

# Watch mode (auto-rebuild)
dotnet watch --project src/VolcanionPM.API
```

### Database (After Entity Configs Fixed)
```bash
# Create migration
dotnet ef migrations add InitialCreate -p src/VolcanionPM.Infrastructure -s src/VolcanionPM.API

# Apply migration
dotnet ef database update -p src/VolcanionPM.Infrastructure -s src/VolcanionPM.API

# Remove last migration
dotnet ef migrations remove -p src/VolcanionPM.Infrastructure -s src/VolcanionPM.API
```

### Package Management
```bash
# Add package
dotnet add src/VolcanionPM.Infrastructure package BCrypt.Net-Next

# Restore packages
dotnet restore

# List packages
dotnet list package
```

---

## 📁 KEY FILE LOCATIONS

### Domain Layer
```
src/VolcanionPM.Domain/
├── Entities/
│   ├── Project.cs          ← Add Update methods here
│   ├── ProjectTask.cs      ← Add Update methods here
│   └── User.cs             ← Add Update methods here
├── ValueObjects/
│   ├── Email.cs
│   ├── Money.cs
│   └── DateRange.cs
└── Enums/
    └── DomainEnums.cs      ← UserRole enum is here
```

### Infrastructure Layer
```
src/VolcanionPM.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── ReadDbContext.cs
│   └── Configurations/     ← CREATE ENTITY CONFIGS HERE
├── Repositories/
│   └── *.cs                ← 7 repository files
└── Services/
    ├── RedisCacheService.cs
    ├── TokenService.cs
    └── PasswordHasher.cs   ← CREATE THIS
```

### Application Layer
```
src/VolcanionPM.Application/
├── Features/
│   ├── Projects/
│   │   ├── Commands/Create/  ← CreateProjectCommand + Handler
│   │   ├── Commands/Update/  ← UpdateProjectCommand + Handler
│   │   └── Queries/          ← GetProjectByIdQuery + Handler
│   ├── Tasks/
│   └── Auth/
├── Common/
│   ├── Interfaces/
│   │   └── IPasswordHasher.cs  ← CREATE THIS
│   ├── Models/
│   │   └── Result.cs
│   └── Behaviors/              ← CREATE PIPELINE BEHAVIORS HERE
└── DTOs/
```

### API Layer
```
src/VolcanionPM.API/
├── Controllers/
│   ├── ProjectsController.cs  ← 5 endpoints
│   ├── TasksController.cs     ← 4 endpoints
│   └── AuthController.cs      ← 3 endpoints
├── Program.cs
└── appsettings.json
```

---

## 🧪 TESTING ENDPOINTS (After Fixes)

### Auth Endpoints
```bash
# Register
POST http://localhost:5000/api/auth/register
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "organizationId": "guid-here"
}

# Login
POST http://localhost:5000/api/auth/login
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

### Project Endpoints
```bash
# Create Project
POST http://localhost:5000/api/projects
Authorization: Bearer {token}
{
  "name": "New Project",
  "code": "PROJ-001",
  "description": "Test project",
  "priority": "High",
  "organizationId": "guid-here"
}

# Get All Projects
GET http://localhost:5000/api/projects
Authorization: Bearer {token}
```

---

## ⚠️ KNOWN ISSUES

### Critical (Fix Immediately)
1. 🔴 Password hashing is placeholder
2. 🔴 Entity configurations missing (can't create DB)
3. 🔴 Update operations don't work

### High Priority (Fix Soon)
4. 🟡 No validation pipeline (ValidationBehavior)
5. 🟡 No authorization (anyone can access anything)
6. 🟡 No pagination (GetAll returns everything)

### Medium Priority (Can Wait)
7. 🟢 No caching implementation
8. 🟢 Missing 8 controllers (Organizations, Users, etc.)
9. 🟢 No unit tests (0% coverage)

---

## 📊 PROGRESS CHECKLIST

### Week 1 (Current)
- [x] Phase 3: Infrastructure complete
- [x] Phase 4: Basic CQRS (40%)
- [ ] Fix password hashing
- [ ] Fix entity configurations
- [ ] Fix domain updates
- [ ] Add validation pipeline
- [ ] Add authorization

### Week 2 (Next)
- [ ] Complete remaining CRUD operations
- [ ] Add pagination
- [ ] Create remaining controllers
- [ ] Implement caching

### Week 3-4
- [ ] Dashboard endpoints
- [ ] Health checks
- [ ] Comprehensive logging
- [ ] Unit tests

### Week 5-6
- [ ] Integration tests
- [ ] Security audit
- [ ] Performance testing
- [ ] Production deployment

---

## 🎯 DAILY GOALS

### Today's Goal
✅ Complete Phases 3-6 (partial) - DONE

### Tomorrow's Goal
1. Fix password hashing (2-3 hours)
2. Recreate entity configurations (4-6 hours)
3. Generate initial migration
4. Verify database creation

### This Week's Goal
- Complete all critical fixes
- Add validation pipeline
- Add basic authorization
- Complete Phase 4-6

---

## 💬 QUICK TIPS

### Development Workflow
1. Always build after changes: `dotnet build`
2. Check for errors frequently
3. Test endpoints with Swagger UI: http://localhost:5000/swagger
4. Use Git commits frequently

### Common Commands
```bash
# Quick build
dotnet build --no-restore

# Clean build
dotnet clean && dotnet build

# Run with hot reload
dotnet watch --project src/VolcanionPM.API

# Check for updates
dotnet list package --outdated
```

### Troubleshooting
- **Build fails**: Check CRITICAL-ISSUES.md
- **Can't create migrations**: Entity configurations missing
- **Updates don't work**: Domain Update methods missing
- **Auth doesn't work**: Password hashing issue

---

## 📞 HELP & SUPPORT

### Documentation Priority
1. Start with CRITICAL-ISSUES.md (immediate fixes)
2. Check TODO.md (complete task list)
3. Follow ROADMAP.md (30-day plan)
4. Reference docs/PHASE-3-6-COMPLETION-SUMMARY.md (what's done)

### Key Concepts
- **CQRS**: Commands (write) vs Queries (read)
- **MediatR**: Request/handler pattern
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Value Objects**: Email, Money, DateRange (use .Create())

---

**Next Session**: Fix password hashing → Entity configs → Domain updates
**Estimated Time**: 12-17 hours (2-3 days)
**Priority**: Critical security and blocking issues first

