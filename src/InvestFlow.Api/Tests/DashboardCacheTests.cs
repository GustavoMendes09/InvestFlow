using InvestFlow.Api.Features.Dashboard;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InvestFlow.Api.Tests;

public sealed class DashboardCacheTests
{
    [Fact]
    public async Task GetOrCreateAsync_ReturnsCachedResponseForSameUserAndMonth()
    {
        var cache = CreateCache();
        var month = new DateOnly(2026, 8, 1);
        var factoryCalls = 0;

        Task<DashboardResponse> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(CreateResponse(month, factoryCalls));
        }

        var first = await cache.GetOrCreateAsync("user-1", month, Factory, CancellationToken.None);
        var second = await cache.GetOrCreateAsync("user-1", month, Factory, CancellationToken.None);

        Assert.Equal(1, factoryCalls);
        Assert.Equal(first.Month, second.Month);
        Assert.Equal(first.Income, second.Income);
        Assert.Empty(second.CategoryImpact);
        Assert.Empty(second.History);
    }

    [Fact]
    public async Task InvalidateAsync_ForcesARefreshForTheUser()
    {
        var cache = CreateCache();
        var month = new DateOnly(2026, 8, 1);
        var factoryCalls = 0;

        Task<DashboardResponse> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(CreateResponse(month, factoryCalls));
        }

        var beforeInvalidation = await cache.GetOrCreateAsync(
            "user-1",
            month,
            Factory,
            CancellationToken.None);
        await cache.InvalidateAsync("user-1", CancellationToken.None);
        var afterInvalidation = await cache.GetOrCreateAsync(
            "user-1",
            month,
            Factory,
            CancellationToken.None);

        Assert.Equal(2, factoryCalls);
        Assert.NotEqual(beforeInvalidation.Income, afterInvalidation.Income);
    }

    [Fact]
    public async Task CacheEntries_AreIsolatedByUserAndMonth()
    {
        var cache = CreateCache();
        var factoryCalls = 0;

        Task<DashboardResponse> Factory(DateOnly month, CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(CreateResponse(month, factoryCalls));
        }

        await cache.GetOrCreateAsync(
            "user-1",
            new DateOnly(2026, 8, 1),
            token => Factory(new DateOnly(2026, 8, 1), token),
            CancellationToken.None);
        await cache.GetOrCreateAsync(
            "user-1",
            new DateOnly(2026, 9, 1),
            token => Factory(new DateOnly(2026, 9, 1), token),
            CancellationToken.None);
        await cache.GetOrCreateAsync(
            "user-2",
            new DateOnly(2026, 8, 1),
            token => Factory(new DateOnly(2026, 8, 1), token),
            CancellationToken.None);

        Assert.Equal(3, factoryCalls);
    }

    [Fact]
    public async Task UnavailableCache_FallsBackToTheFactory()
    {
        var cache = new DashboardCache(
            new UnavailableDistributedCache(),
            Options.Create(new DashboardCacheOptions()),
            NullLogger<DashboardCache>.Instance);
        var factoryCalls = 0;

        var response = await cache.GetOrCreateAsync(
            "user-1",
            new DateOnly(2026, 8, 1),
            _ =>
            {
                factoryCalls++;
                return Task.FromResult(CreateResponse(new DateOnly(2026, 8, 1), 5_000));
            },
            CancellationToken.None);

        Assert.Equal(1, factoryCalls);
        Assert.Equal(5_000, response.Income);
    }

    private static DashboardCache CreateCache()
    {
        var distributedCache = new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions()));
        return new DashboardCache(
            distributedCache,
            Options.Create(new DashboardCacheOptions()),
            NullLogger<DashboardCache>.Instance);
    }

    private static DashboardResponse CreateResponse(DateOnly month, decimal income) =>
        new(
            month,
            income,
            0,
            income,
            0,
            0,
            0,
            0,
            [],
            []);

    private sealed class UnavailableDistributedCache : IDistributedCache
    {
        public byte[]? Get(string key) => throw new InvalidOperationException("Cache unavailable.");

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            Task.FromException<byte[]?>(new InvalidOperationException("Cache unavailable."));

        public void Refresh(string key) => throw new InvalidOperationException("Cache unavailable.");

        public Task RefreshAsync(string key, CancellationToken token = default) =>
            Task.FromException(new InvalidOperationException("Cache unavailable."));

        public void Remove(string key) => throw new InvalidOperationException("Cache unavailable.");

        public Task RemoveAsync(string key, CancellationToken token = default) =>
            Task.FromException(new InvalidOperationException("Cache unavailable."));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            throw new InvalidOperationException("Cache unavailable.");

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default) =>
            Task.FromException(new InvalidOperationException("Cache unavailable."));
    }
}
