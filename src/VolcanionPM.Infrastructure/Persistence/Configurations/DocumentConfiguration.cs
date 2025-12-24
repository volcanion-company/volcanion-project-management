using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.Property(d => d.Version)
            .HasMaxLength(50);

        // Audit fields
        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.UpdatedAt);

        builder.Property(d => d.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.DeletedAt);

        builder.Property(d => d.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(d => d.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);

        // Indexes
        builder.HasIndex(d => d.ProjectId);
        builder.HasIndex(d => d.UploadedById);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.CreatedAt);
    }
}
