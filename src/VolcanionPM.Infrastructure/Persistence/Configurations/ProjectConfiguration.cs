using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.ProgressPercentage)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        // DateRange value object
        builder.OwnsOne(p => p.DateRange, dateRange =>
        {
            dateRange.Property(d => d.StartDate)
                .IsRequired()
                .HasColumnName("StartDate");

            dateRange.Property(d => d.EndDate)
                .IsRequired()
                .HasColumnName("EndDate");
        });

        // Budget (Money value object)
        builder.OwnsOne(p => p.Budget, budget =>
        {
            budget.Property(b => b.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnName("BudgetAmount");

            budget.Property(b => b.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("BudgetCurrency");
        });

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedAt);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(p => p.Organization)
            .WithMany(o => o.Projects)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProjectManager)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Sprints)
            .WithOne(s => s.Project)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Risks)
            .WithOne(r => r.Project)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Issues)
            .WithOne(i => i.Project)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Documents)
            .WithOne(d => d.Project)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ResourceAllocations)
            .WithOne(ra => ra.Project)
            .HasForeignKey(ra => ra.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Indexes
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.OrganizationId);
        builder.HasIndex(p => p.ProjectManagerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Priority);
        builder.HasIndex(p => p.CreatedAt);
        
        // Composite indexes for common query patterns
        builder.HasIndex(p => new { p.OrganizationId, p.Status });
        builder.HasIndex(p => new { p.OrganizationId, p.Priority });
        builder.HasIndex(p => new { p.Status, p.Priority });
        builder.HasIndex(p => new { p.ProjectManagerId, p.Status });
        
        // Covering index for list queries (includes Name for sorting)
        builder.HasIndex(p => new { p.OrganizationId, p.Status, p.CreatedAt });
    }
}
