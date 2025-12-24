using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Enums;
using VolcanionPM.Domain.ValueObjects;
using VolcanionPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskStatus = VolcanionPM.Domain.Enums.TaskStatus;

namespace VolcanionPM.Infrastructure.Seeders;

/// <summary>
/// Seeds initial data for development and testing
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (await _context.Users.AnyAsync())
        {
            return; // Database is already seeded
        }

        await SeedOrganizationsAsync();
        await SeedUsersAsync();
        await SeedProjectsAsync();
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedOrganizationsAsync()
    {
        var organizations = new[]
        {
            Organization.Create(
                name: "Volcanion Technologies",
                description: "Leading technology solutions provider",
                website: "https://volcanion.tech",
                address: Address.Create(
                    street: "123 Tech Street",
                    city: "San Francisco",
                    state: "CA",
                    country: "USA",
                    postalCode: "94105"
                )
            ),
            Organization.Create(
                name: "Acme Corporation",
                description: "Enterprise software development company",
                website: "https://acme.corp",
                address: Address.Create(
                    street: "456 Business Ave",
                    city: "New York",
                    state: "NY",
                    country: "USA",
                    postalCode: "10001"
                )
            )
        };

        await _context.Organizations.AddRangeAsync(organizations);
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var volcanion = await _context.Organizations
            .FirstAsync(o => o.Name == "Volcanion Technologies");
        
        var acme = await _context.Organizations
            .FirstAsync(o => o.Name == "Acme Corporation");

        var users = new[]
        {
            // Volcanion Users
            User.Create(
                firstName: "Admin",
                lastName: "User",
                email: "admin@volcanion.tech",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                organizationId: volcanion.Id,
                role: UserRole.SystemAdmin
            ),
            User.Create(
                firstName: "John",
                lastName: "Manager",
                email: "john.manager@volcanion.tech",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Manager@123", workFactor: 12),
                organizationId: volcanion.Id,
                role: UserRole.ProjectManager
            ),
            User.Create(
                firstName: "Sarah",
                lastName: "Developer",
                email: "sarah.dev@volcanion.tech",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Dev@123", workFactor: 12),
                organizationId: volcanion.Id,
                role: UserRole.Developer
            ),
            User.Create(
                firstName: "Mike",
                lastName: "Tester",
                email: "mike.tester@volcanion.tech",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Test@123", workFactor: 12),
                organizationId: volcanion.Id,
                role: UserRole.Tester
            ),
            
            // Acme Users
            User.Create(
                firstName: "Jane",
                lastName: "Smith",
                email: "jane.smith@acme.corp",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Acme@123", workFactor: 12),
                organizationId: acme.Id,
                role: UserRole.ProjectManager
            ),
            User.Create(
                firstName: "Bob",
                lastName: "Johnson",
                email: "bob.johnson@acme.corp",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Acme@123", workFactor: 12),
                organizationId: acme.Id,
                role: UserRole.Developer
            )
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    private async Task SeedProjectsAsync()
    {
        var volcanion = await _context.Organizations
            .FirstAsync(o => o.Name == "Volcanion Technologies");
        
        var manager = await _context.Users
            .FirstAsync(u => u.Email.Value == "john.manager@volcanion.tech");

        var projects = new[]
        {
            Project.Create(
                name: "Project Management System",
                code: "PMS-2025",
                organizationId: volcanion.Id,
                projectManagerId: manager.Id,
                dateRange: DateRange.Create(DateTime.UtcNow, DateTime.UtcNow.AddMonths(6)),
                budget: Money.Create(150000m, "USD"),
                priority: ProjectPriority.High,
                description: "Modern project management platform with advanced features"
            ),
            Project.Create(
                name: "Mobile App Development",
                code: "MOBILE-2025",
                organizationId: volcanion.Id,
                projectManagerId: manager.Id,
                dateRange: DateRange.Create(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddMonths(4)),
                budget: Money.Create(80000m, "USD"),
                priority: ProjectPriority.Medium,
                description: "Cross-platform mobile application for iOS and Android"
            ),
            Project.Create(
                name: "API Gateway",
                code: "API-GW-2025",
                organizationId: volcanion.Id,
                projectManagerId: manager.Id,
                dateRange: DateRange.Create(DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(3)),
                budget: Money.Create(50000m, "USD"),
                priority: ProjectPriority.Low,
                description: "Microservices API gateway implementation"
            )
        };

        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        // Seed some tasks for the first project
        var pmsProject = projects[0];
        var developer = await _context.Users
            .FirstAsync(u => u.Email.Value == "sarah.dev@volcanion.tech");

        var tasks = new[]
        {
            ProjectTask.Create(
                title: "Setup Project Infrastructure",
                code: "PMS-001",
                projectId: pmsProject.Id,
                type: TaskType.Task,
                priority: TaskPriority.High,
                estimatedHours: 16m,
                description: "Initialize solution structure, configure CI/CD",
                assignedToId: developer.Id
            ),
            ProjectTask.Create(
                title: "Implement User Authentication",
                code: "PMS-002",
                projectId: pmsProject.Id,
                type: TaskType.Feature,
                priority: TaskPriority.High,
                estimatedHours: 24m,
                description: "JWT-based authentication with refresh tokens",
                assignedToId: developer.Id
            ),
            ProjectTask.Create(
                title: "Design Database Schema",
                code: "PMS-003",
                projectId: pmsProject.Id,
                type: TaskType.Task,
                priority: TaskPriority.Medium,
                estimatedHours: 12m,
                description: "Entity relationship diagram and migrations",
                assignedToId: developer.Id
            )
        };

        // Update task statuses
        tasks[0].ChangeStatus(TaskStatus.Done, "System");
        tasks[1].ChangeStatus(TaskStatus.InProgress, "System");

        await _context.Tasks.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();
    }
}
