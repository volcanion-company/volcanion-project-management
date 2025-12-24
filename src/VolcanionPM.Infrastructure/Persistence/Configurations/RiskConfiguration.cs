using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class RiskConfiguration : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Level)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Probability)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(r => r.Impact)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(r => r.MitigationStrategy)
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.DeletedAt);

        builder.Property(r => r.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(r => r.Project)
            .WithMany(p => p.Risks)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Owner)
            .WithMany()
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Query filter for soft delete
        builder.HasQueryFilter(r => !r.IsDeleted);

        // Indexes
        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => r.Level);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);
    }
}
