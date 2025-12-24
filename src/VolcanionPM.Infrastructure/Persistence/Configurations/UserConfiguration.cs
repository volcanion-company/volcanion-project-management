using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionPM.Domain.Entities;
using VolcanionPM.Domain.ValueObjects;

namespace VolcanionPM.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Use lowercase table name to match init-db.sql
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        
        // Map all properties to lowercase column names
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");

        // Email value object - map to lowercase
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("uq_users_email");
        });

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("password_hash");

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20)
            .HasColumnName("phone_number");

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500)
            .HasColumnName("profile_picture_url");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(u => u.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("email_confirmed");

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500)
            .HasColumnName("refresh_token");

        builder.Property(u => u.RefreshTokenExpiryDate)
            .HasColumnName("refresh_token_expires_at");

        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(10)
            .HasColumnName("password_reset_token");

        builder.Property(u => u.PasswordResetTokenExpiryDate)
            .HasColumnName("password_reset_token_expires_at");

        // Account lockout fields
        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("failed_login_attempts");

        builder.Property(u => u.LastFailedLoginAt)
            .HasColumnName("last_failed_login_at");

        builder.Property(u => u.LockoutEndDate)
            .HasColumnName("lockout_end");

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("role");

        // Audit fields
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100)
            .HasColumnName("updated_by");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_deleted");

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(100)
            .HasColumnName("deleted_by");

        builder.Property(u => u.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        // Relationships
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.OwnedProjects)
            .WithOne(p => p.ProjectManager)
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedTasks)
            .WithOne(t => t.AssignedTo)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.TimeEntries)
            .WithOne(te => te.User)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Indexes
        builder.HasIndex(u => u.OrganizationId);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.Role);
        
        // Composite indexes for common query patterns
        builder.HasIndex(u => new { u.OrganizationId, u.IsActive });
        builder.HasIndex(u => new { u.OrganizationId, u.Role });
        builder.HasIndex(u => new { u.IsActive, u.Role });
    }
}
