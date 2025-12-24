using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasMaxLength(2000);

        builder.Property(o => o.Website)
            .HasMaxLength(500);

        builder.Property(o => o.LogoUrl)
            .HasMaxLength(500);

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Address value object
        builder.OwnsOne(o => o.Address, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(200)
                .HasColumnName("Street");

            address.Property(a => a.City)
                .HasMaxLength(100)
                .HasColumnName("City");

            address.Property(a => a.State)
                .HasMaxLength(100)
                .HasColumnName("State");

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("PostalCode");

            address.Property(a => a.Country)
                .HasMaxLength(100)
                .HasColumnName("Country");
        });

        // Audit fields
        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.DeletedAt);

        builder.Property(o => o.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Projects)
            .WithOne(p => p.Organization)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(o => !o.IsDeleted);

        // Indexes
        builder.HasIndex(o => o.Name);
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.CreatedAt);
    }
}
