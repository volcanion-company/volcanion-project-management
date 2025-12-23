using Microsoft.EntityFrameworkCore;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.Common;
using System.Reflection;

namespace VolcanionPM.Infrastructure.Persistence;

/// <summary>
/// Main database context for write operations
/// Implements domain event publishing and audit tracking
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Aggregate Roots
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();

    // Child Entities
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure schema
        modelBuilder.HasDefaultSchema("volcanion_pm");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields before saving
        UpdateAuditFields();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        await PublishDomainEventsAsync(cancellationToken);

        return result;
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        // TODO: Publish events using MediatR or message bus
        // For now, events are just cleared after save
        await Task.CompletedTask;
    }
}
