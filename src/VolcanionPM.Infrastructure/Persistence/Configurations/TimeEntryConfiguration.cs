using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.Description)
            .HasMaxLength(1000);

        builder.Property(te => te.Hours)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(te => te.Date)
            .IsRequired();

        builder.Property(te => te.IsBillable)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit fields
        builder.Property(te => te.CreatedAt)
            .IsRequired();

        builder.Property(te => te.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(te => te.UpdatedAt);

        builder.Property(te => te.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(te => te.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(te => te.DeletedAt);

        builder.Property(te => te.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(te => te.User)
            .WithMany(u => u.TimeEntries)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(te => te.Task)
            .WithMany(t => t.TimeEntries)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        builder.HasQueryFilter(te => !te.IsDeleted);

        // Indexes
        builder.HasIndex(te => te.UserId);
        builder.HasIndex(te => te.TaskId);
        builder.HasIndex(te => te.Date);
        builder.HasIndex(te => te.CreatedAt);
        builder.HasIndex(te => te.IsBillable);
        
        // Composite indexes for time tracking queries
        builder.HasIndex(te => new { te.UserId, te.Date });
        builder.HasIndex(te => new { te.TaskId, te.Date });
        builder.HasIndex(te => new { te.UserId, te.IsBillable });
    }
}
