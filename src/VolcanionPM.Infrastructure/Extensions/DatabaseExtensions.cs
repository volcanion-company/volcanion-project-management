using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolcanionPM.Infrastructure.Persistence;
using VolcanionPM.Infrastructure.Seeders;

namespace VolcanionPM.Infrastructure.Extensions;

/// <summary>
/// Extension methods for database initialization
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Applies pending migrations and seeds initial data (development only)
    /// </summary>
    public static async Task<IApplicationBuilder> InitializeDatabaseAsync(
        this IApplicationBuilder app,
        bool isDevelopment = false)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Comment out migrations since we're using init-db.sql for schema creation
            // Apply pending migrations
            // logger.LogInformation("Checking for pending database migrations...");
            // var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            //
            // if (pendingMigrations.Any())
            // {
            //     logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count());
            //     await context.Database.MigrateAsync();
            //     logger.LogInformation("Database migrations applied successfully");
            // }
            // else
            // {
            //     logger.LogInformation("Database is up to date");
            // }

            // Seed data in development environment
            if (isDevelopment)
            {
                logger.LogInformation("Seeding development data...");
                var seeder = new DatabaseSeeder(context);
                await seeder.SeedAsync();
                logger.LogInformation("Development data seeded successfully");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            
            // Don't throw in production - let the app start and handle database errors gracefully
            if (isDevelopment)
            {
                throw;
            }
        }

        return app;
    }

    /// <summary>
    /// Ensures the database exists and is accessible
    /// </summary>
    public static async Task<bool> CanConnectToDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var canConnect = await context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                logger.LogInformation("Database connection established successfully");
            }
            else
            {
                logger.LogWarning("Unable to establish database connection");
            }

            return canConnect;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing database connection");
            return false;
        }
    }
}
