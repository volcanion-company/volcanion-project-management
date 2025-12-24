using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class ResourceAllocationConfiguration : IEntityTypeConfiguration<ResourceAllocation>
{
    public void Configure(EntityTypeBuilder<ResourceAllocation> builder)
    {
        builder.ToTable("ResourceAllocations");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ra => ra.AllocationPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(ra => ra.Notes)
            .HasMaxLength(1000);

        // DateRange value object
        builder.OwnsOne(ra => ra.AllocationPeriod, dateRange =>
        {
            dateRange.Property(d => d.StartDate)
                .IsRequired()
                .HasColumnName("StartDate");

            dateRange.Property(d => d.EndDate)
                .IsRequired()
                .HasColumnName("EndDate");
        });

        // HourlyRate (Money value object)
        builder.OwnsOne(ra => ra.HourlyRate, money =>
        {
            money.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("HourlyRateAmount");

            money.Property(m => m.Currency)
                .HasMaxLength(3)
                .HasColumnName("HourlyRateCurrency");
        });

        // Audit fields
        builder.Property(ra => ra.CreatedAt)
            .IsRequired();

        builder.Property(ra => ra.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ra => ra.UpdatedAt);

        builder.Property(ra => ra.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(ra => ra.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ra => ra.DeletedAt);

        builder.Property(ra => ra.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(ra => ra.User)
            .WithMany()
            .HasForeignKey(ra => ra.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.Project)
            .WithMany(p => p.ResourceAllocations)
            .HasForeignKey(ra => ra.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        builder.HasQueryFilter(ra => !ra.IsDeleted);

        // Indexes
        builder.HasIndex(ra => ra.UserId);
        builder.HasIndex(ra => ra.ProjectId);
        builder.HasIndex(ra => ra.CreatedAt);
    }
}
