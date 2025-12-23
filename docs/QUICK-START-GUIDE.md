# 🚀 QUICK START GUIDE

## For Developers New to This Project

---

## 📁 Project Structure

```
volcanion-project-management/
│
├── src/
│   ├── VolcanionPM.Domain/         ← Start here: Business logic
│   ├── VolcanionPM.Application/    ← Next: Use cases
│   ├── VolcanionPM.Infrastructure/ ← Then: Data access
│   └── VolcanionPM.API/            ← Finally: HTTP endpoints
│
├── docs/                            ← Read first!
│   ├── Phase-1-Architecture-Setup.md
│   ├── Phase-2-Domain-Layer.md
│   ├── PHASE-1-2-COMPLETION-SUMMARY.md
│   └── PROJECT-ROADMAP.md
│
└── README.md                        ← Start here!
```

---

## 🎯 Understanding the Architecture

### Layer Flow (Clean Architecture)
```
API Layer → Application Layer → Infrastructure Layer → Domain Layer
   ↓              ↓                    ↓                   ↑
HTTP          Commands/           Repositories         Entities
Requests      Queries             & UoW                & Rules
```

### Dependency Rule
```
Domain ← Application ← Infrastructure ← API
(No dependencies)  (Depends on Domain)  (Depends on App+Domain)  (Depends on all)
```

**Key**: Inner layers never depend on outer layers!

---

## 🔑 Key Concepts

### 1. Aggregate Roots (Phase 2 ✅)
These are the main entities:
- **Organization** - Company/team using the system
- **User** - System user with auth
- **Project** - Main project entity

Access other entities through these roots only!

### 2. Value Objects (Phase 2 ✅)
Immutable, behavior-rich objects:
```csharp
var email = Email.Create("user@example.com");
var budget = Money.Create(50000, "USD");
var period = DateRange.Create(startDate, endDate);
```

### 3. Domain Events (Phase 2 ✅)
Important things that happened:
```csharp
public record ProjectCreatedEvent(
    Guid ProjectId,
    string ProjectName,
    string ProjectCode) : IDomainEvent;
```

### 4. CQRS (Phase 4 - Coming)
- **Commands**: Change state (Write)
- **Queries**: Read data (Read)

Separate read/write concerns!

---

## 📚 Read These First

### Priority Order:
1. **README.md** - Project overview
2. **Phase-1-Architecture-Setup.md** - Understand structure
3. **Phase-2-Domain-Layer.md** - Understand business logic
4. **PHASE-1-2-COMPLETION-SUMMARY.md** - What's done
5. **PROJECT-ROADMAP.md** - What's coming

---

## 🏃 Common Tasks

### Creating a New Entity
```csharp
// 1. Create in Domain/Entities/
public class MyEntity : BaseEntity  // or AggregateRoot
{
    // Private setters!
    public string Name { get; private set; }
    
    // Factory method
    public static MyEntity Create(string name, ...)
    {
        // Validation
        var entity = new MyEntity { Name = name };
        
        // Raise domain event
        entity.AddDomainEvent(new MyEntityCreatedEvent(...));
        
        return entity;
    }
    
    // Business methods
    public void UpdateName(string newName, string updatedBy)
    {
        // Validation
        Name = newName;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Creating a Value Object
```csharp
public class MyValueObject : ValueObject
{
    public string Value { get; private set; }
    
    private MyValueObject(string value) => Value = value;
    
    public static MyValueObject Create(string value)
    {
        // Validation
        return new MyValueObject(value);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Adding a Domain Event
```csharp
public record MyDomainEvent(
    Guid EntityId,
    string Data) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
```

---

## 🔍 Where to Find Things

### Business Logic?
→ `Domain/Entities/`

### Business Rules?
→ Inside entity methods in `Domain/Entities/`

### Enums?
→ `Domain/Enums/DomainEnums.cs`

### Value Objects?
→ `Domain/ValueObjects/`

### Database Configuration?
→ `appsettings.json` (coming in Phase 3)

### API Endpoints?
→ `API/Controllers/` (coming in Phase 6)

### Authentication?
→ `Infrastructure/Services/TokenService.cs` (coming in Phase 5)

---

## 🎨 Coding Standards

### Entity Guidelines
✅ Private setters  
✅ Factory methods for creation  
✅ Business methods for operations  
✅ Validation in methods  
✅ Raise domain events  
✅ No anemic models  

### Value Object Guidelines
✅ Immutable  
✅ Validation in Create method  
✅ Override equality  
✅ No identity  

### Naming Conventions
✅ PascalCase for classes, methods, properties  
✅ camelCase for parameters  
✅ _camelCase for private fields  
✅ Meaningful names (no abbreviations)  
✅ Verbs for methods: `CreateProject`, `AssignTask`  

---

## 🚫 Common Mistakes to Avoid

❌ **DON'T** add dependencies from Domain to other layers  
❌ **DON'T** use public setters on entities  
❌ **DON'T** bypass aggregate roots  
❌ **DON'T** create anemic entities (just getters/setters)  
❌ **DON'T** put business logic in Application or Infrastructure  
❌ **DON'T** forget to raise domain events  
❌ **DON'T** skip validation  

✅ **DO** use factory methods  
✅ **DO** validate in domain  
✅ **DO** use value objects for domain concepts  
✅ **DO** raise domain events  
✅ **DO** follow SOLID principles  

---

## 🔧 Development Setup (Coming Soon)

### Prerequisites
- .NET 10 SDK
- PostgreSQL 14+
- Redis 6+
- Visual Studio 2022 / VS Code / Rider

### Running the Project (After Phase 6)
```bash
# Restore packages
dotnet restore

# Run migrations
dotnet ef database update --project src/VolcanionPM.Infrastructure

# Run API
dotnet run --project src/VolcanionPM.API
```

---

## 📖 Learning Resources

### Clean Architecture
- [Microsoft Clean Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- Book: "Clean Architecture" by Robert C. Martin

### Domain-Driven Design
- Book: "Domain-Driven Design" by Eric Evans
- Book: "Implementing Domain-Driven Design" by Vaughn Vernon

### CQRS
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Martin Fowler on CQRS](https://martinfowler.com/bliki/CQRS.html)

---

## 💡 Tips

### Understanding the Domain
1. Start with `Domain/Entities/`
2. Read entity methods to understand business rules
3. Check domain events to see what happens when
4. Look at enums for domain concepts

### Adding Features
1. Start with domain entities (Phase 2)
2. Add repository interfaces (Phase 3)
3. Create commands/queries (Phase 4)
4. Add API endpoints (Phase 6)

### Debugging
- Check logs (Serilog)
- Use Correlation ID to trace requests
- Swagger for API testing
- Domain events show what happened

---

## 🆘 Need Help?

### Questions About:
- **Architecture**: Read Phase-1-Architecture-Setup.md
- **Domain Model**: Read Phase-2-Domain-Layer.md
- **What's Done**: Read PHASE-1-2-COMPLETION-SUMMARY.md
- **What's Next**: Read PROJECT-ROADMAP.md

### Still Stuck?
- Check entity examples in `Domain/Entities/`
- Look at value object examples in `Domain/ValueObjects/`
- Review middleware in `API/Middleware/`

---

## 🎯 Your First Contribution

### Understanding Phase (1-2 days)
1. ✅ Read README.md
2. ✅ Read Phase-1-Architecture-Setup.md
3. ✅ Read Phase-2-Domain-Layer.md
4. ✅ Browse Domain entities
5. ✅ Understand relationships

### Ready to Code? (Phase 3 Next!)
After understanding, you'll be ready for:
- Repository implementations
- Database configurations
- Unit of Work
- Cache services

---

## 📊 Current Project Status

**Completed**: Phase 1 + 2  
**Next**: Phase 3 - Infrastructure Layer  
**Progress**: 20% (2/10 phases)  

See [PROJECT-ROADMAP.md](./PROJECT-ROADMAP.md) for details.

---

## 🎉 Welcome to the Team!

This is an enterprise-grade project following best practices. Take your time to understand the architecture—it will make you a better developer!

---

**Last Updated**: December 23, 2025  
**For**: Volcanion Project Management System  
**Version**: 1.0.0-alpha
