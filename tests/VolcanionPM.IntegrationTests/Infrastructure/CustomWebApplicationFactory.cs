using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VolcanionPM.Infrastructure.Persistence;

namespace VolcanionPM.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests with in-memory database.
/// Replaces PostgreSQL with in-memory database for testing.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove DbContext registrations - must remove ALL related types
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ReadDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<ReadDbContext>();
            
            // Also remove the generic DbContextOptions that might be registered
            var dbContextOptionsDescriptors = services
                .Where(d => d.ServiceType.IsGenericType && 
                           d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                .ToList();
            foreach (var descriptor in dbContextOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing - use separate databases
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("VolcanionPMTestDb"));
            
            services.AddDbContext<ReadDbContext>(options =>
                options.UseInMemoryDatabase("VolcanionPMTestDb"));

            // Remove Redis cache service and replace with null object/mock
            services.RemoveAll(typeof(VolcanionPM.Application.Common.Interfaces.ICacheService));
            services.AddSingleton<VolcanionPM.Application.Common.Interfaces.ICacheService, NoCacheService>();
        });

        builder.UseEnvironment("Testing");
    }
}

/// <summary>
/// No-op cache service for testing (bypasses Redis).
/// </summary>
public class NoCacheService : VolcanionPM.Application.Common.Interfaces.ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // Always create (no caching in tests)
        return await factory(cancellationToken);
    }
}
