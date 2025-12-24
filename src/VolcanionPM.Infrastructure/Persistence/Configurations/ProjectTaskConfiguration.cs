using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("ProjectTasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.EstimatedHours)
            .HasPrecision(8, 2);

        builder.Property(t => t.ActualHours)
            .HasPrecision(8, 2);

        builder.Property(t => t.StoryPoints);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(50);

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Sprint)
            .WithMany(s => s.Tasks)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.TimeEntries)
            .WithOne(te => te.Task)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Task)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        builder.HasQueryFilter(t => !t.IsDeleted);

        // Indexes
        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.SprintId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.Type);
        
        // Composite indexes for common query patterns
        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => new { t.ProjectId, t.Priority });
        builder.HasIndex(t => new { t.ProjectId, t.AssignedToId });
        builder.HasIndex(t => new { t.AssignedToId, t.Status });
        builder.HasIndex(t => new { t.SprintId, t.Status });
        builder.HasIndex(t => new { t.Status, t.Priority });
        
        // Covering index for sprint queries
        builder.HasIndex(t => new { t.ProjectId, t.Status, t.DueDate });
    }
}
