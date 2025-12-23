using Microsoft.EntityFrameworkCore;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence;

/// <summary>
/// Read-only database context for query operations
/// Optimized for read performance with no change tracking
/// </summary>
public class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) 
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
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

        // Use same configuration as write context
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema("volcanion_pm");
    }
}
