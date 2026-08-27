using InvestFlow.Api.Domain.Finance;
using InvestFlow.Api.Domain.Snapshots;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Dashboard;

public sealed class NetWorthSnapshotService(
    AppDbContext database,
    TimeProvider timeProvider,
    DashboardCache dashboardCache)
{
    public async Task UpdateCurrentAsync(string userId, CancellationToken cancellationToken)
    {
        var accounts = await database.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .ToListAsync(cancellationToken);
        var investments = await database.Investments
            .AsNoTracking()
            .Where(investment => investment.UserId == userId)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        var month = new DateOnly(today.Year, today.Month, 1);
        var netWorth = FinancialCalculator.CalculateNetWorth(accounts, investments);
        var snapshot = await database.MonthlySnapshots.FirstOrDefaultAsync(
            item => item.UserId == userId && item.Month == month,
            cancellationToken);

        if (snapshot is null)
        {
            database.MonthlySnapshots.Add(new MonthlySnapshot
            {
                UserId = userId,
                Month = month,
                NetWorth = netWorth
            });
        }
        else
        {
            snapshot.NetWorth = netWorth;
        }

        await database.SaveChangesAsync(cancellationToken);
        await dashboardCache.InvalidateAsync(userId, cancellationToken);
    }
}
