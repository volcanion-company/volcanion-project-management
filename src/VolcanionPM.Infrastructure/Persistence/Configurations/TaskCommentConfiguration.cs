using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("TaskComments");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        // Audit fields
        builder.Property(tc => tc.CreatedAt)
            .IsRequired();

        builder.Property(tc => tc.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.UpdatedAt);

        builder.Property(tc => tc.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(tc => tc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tc => tc.DeletedAt);

        builder.Property(tc => tc.DeletedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(tc => tc.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tc => tc.Author)
            .WithMany()
            .HasForeignKey(tc => tc.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(tc => !tc.IsDeleted);

        // Indexes
        builder.HasIndex(tc => tc.TaskId);
        builder.HasIndex(tc => tc.AuthorId);
        builder.HasIndex(tc => tc.CreatedAt);
    }
}
