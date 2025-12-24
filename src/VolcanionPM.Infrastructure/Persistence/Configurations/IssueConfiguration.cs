using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Resolution)
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.UpdatedAt);

        builder.Property(i => i.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.DeletedAt);

        builder.Property(i => i.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(i => i.Project)
            .WithMany(p => p.Issues)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ReportedBy)
            .WithMany()
            .HasForeignKey(i => i.ReportedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AssignedTo)
            .WithMany()
            .HasForeignKey(i => i.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Query filter for soft delete
        builder.HasQueryFilter(i => !i.IsDeleted);

        // Indexes
        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.ReportedById);
        builder.HasIndex(i => i.AssignedToId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Severity);
        builder.HasIndex(i => i.CreatedAt);
        
        // Composite indexes for issue tracking
        builder.HasIndex(i => new { i.ProjectId, i.Status });
        builder.HasIndex(i => new { i.ProjectId, i.Severity });
        builder.HasIndex(i => new { i.AssignedToId, i.Status });
        builder.HasIndex(i => new { i.Status, i.Severity });
    }
}
