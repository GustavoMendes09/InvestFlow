using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Domain.Snapshots;
using InvestFlow.Api.Infrastructure.Persistence;
using InvestFlow.Api.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvestFlow.Api.IntegrationTests.Integration;

[Collection(ApiTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class PersistenceIntegrationTests(ApiFixture fixture)
{
    [Fact]
    public async Task Startup_AppliesTheEntityFrameworkMigrations()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var migrations = await database.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Contains(migrations, migration => migration.EndsWith("InitialCreate", StringComparison.Ordinal));
    }

    [Fact]
    public async Task MonthlySnapshot_EnforcesOneValuePerUserAndMonth()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = $"constraint-{Guid.NewGuid():N}";
        var month = new DateOnly(2026, 8, 1);

        database.MonthlySnapshots.Add(new MonthlySnapshot
        {
            UserId = userId,
            Month = month,
            NetWorth = 10_000
        });
        await database.SaveChangesAsync(TestContext.Current.CancellationToken);

        database.MonthlySnapshots.Add(new MonthlySnapshot
        {
            UserId = userId,
            Month = month,
            NetWorth = 12_000
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => database.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeletingInvestment_CascadesToItsContributions()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var investment = new Investment
        {
            UserId = $"cascade-{Guid.NewGuid():N}",
            Name = "Index fund",
            AssetClass = "ETF",
            InvestedAmount = 1_000,
            CurrentValue = 1_100,
            UpdatedAt = new DateOnly(2026, 8, 26)
        };
        var contribution = new InvestmentContribution
        {
            Investment = investment,
            Amount = 100,
            Date = new DateOnly(2026, 8, 15)
        };
        database.AddRange(investment, contribution);
        await database.SaveChangesAsync(TestContext.Current.CancellationToken);

        database.Investments.Remove(investment);
        await database.SaveChangesAsync(TestContext.Current.CancellationToken);
        database.ChangeTracker.Clear();

        Assert.False(await database.InvestmentContributions.AnyAsync(
            item => item.Id == contribution.Id,
            TestContext.Current.CancellationToken));
    }
}
