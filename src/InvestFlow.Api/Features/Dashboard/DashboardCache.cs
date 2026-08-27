using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace InvestFlow.Api.Features.Dashboard;

public sealed class DashboardCache(
    IDistributedCache cache,
    IOptions<DashboardCacheOptions> options,
    ILogger<DashboardCache> logger)
{
    private const string InitialGeneration = "initial";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan expiration = options.Value.Expiration > TimeSpan.Zero
        ? options.Value.Expiration
        : TimeSpan.FromMinutes(5);

    public async Task<DashboardResponse> GetOrCreateAsync(
        string userId,
        DateOnly month,
        Func<CancellationToken, Task<DashboardResponse>> factory,
        CancellationToken cancellationToken)
    {
        var generation = await TryGetGenerationAsync(userId, cancellationToken);
        if (generation is null)
        {
            return await factory(cancellationToken);
        }

        var cacheKey = CreateDashboardKey(userId, generation, month);
        var cachedResponse = await TryGetAsync(cacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var response = await factory(cancellationToken);
        await TrySetAsync(cacheKey, response, cancellationToken);
        return response;
    }

    public async Task InvalidateAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            await cache.SetStringAsync(
                CreateGenerationKey(userId),
                Guid.NewGuid().ToString("N"),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration + TimeSpan.FromDays(1)
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not invalidate the dashboard cache for user {UserId}.", userId);
        }
    }

    private async Task<string?> TryGetGenerationAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetStringAsync(CreateGenerationKey(userId), cancellationToken)
                ?? InitialGeneration;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Dashboard cache is unavailable. Falling back to PostgreSQL.");
            return null;
        }
    }

    private async Task<DashboardResponse?> TryGetAsync(
        string cacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var cachedValue = await cache.GetStringAsync(cacheKey, cancellationToken);
            return cachedValue is null
                ? null
                : JsonSerializer.Deserialize<DashboardResponse>(cachedValue, SerializerOptions);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not read dashboard cache entry {CacheKey}.", cacheKey);
            return null;
        }
    }

    private async Task TrySetAsync(
        string cacheKey,
        DashboardResponse response,
        CancellationToken cancellationToken)
    {
        try
        {
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response, SerializerOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                },
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not write dashboard cache entry {CacheKey}.", cacheKey);
        }
    }

    private static string CreateGenerationKey(string userId) =>
        $"dashboard:generation:{userId}";

    private static string CreateDashboardKey(string userId, string generation, DateOnly month) =>
        $"dashboard:{userId}:{generation}:{month:yyyy-MM}";
}
