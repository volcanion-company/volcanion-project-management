using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.ToTable("Sprints");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Goal)
            .HasMaxLength(1000);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // DateRange value object
        builder.OwnsOne(s => s.DateRange, dateRange =>
        {
            dateRange.Property(d => d.StartDate)
                .IsRequired()
                .HasColumnName("StartDate");

            dateRange.Property(d => d.EndDate)
                .IsRequired()
                .HasColumnName("EndDate");
        });

        // Audit fields
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.DeletedAt);

        builder.Property(s => s.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Tasks)
            .WithOne(t => t.Sprint)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);

        // Query filter for soft delete
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Indexes
        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);
        
        // Composite indexes for sprint queries
        builder.HasIndex(s => new { s.ProjectId, s.Status });
    }
}
