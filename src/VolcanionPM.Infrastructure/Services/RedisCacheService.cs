using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.Infrastructure.Services;

/// <summary>
/// Redis-based distributed cache service implementation
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(cached))
            return null;

        return JsonSerializer.Deserialize<T>(cached, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15) // Default 15 minutes
        };

        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, serialized, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis-specific implementation
        // For now, this is a placeholder
        // TODO: Implement pattern-based key deletion using StackExchange.Redis IConnectionMultiplexer
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(cached);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<CancellationToken, Task<T>> factory, 
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
            return cachedValue;

        // If not in cache, create it
        var value = await factory(cancellationToken);

        // Store in cache
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }
}
