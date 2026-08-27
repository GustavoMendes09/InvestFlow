using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Finance;
using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Dashboard;

public sealed class DashboardService(AppDbContext database, DashboardCache cache)
{
    public Task<DashboardResponse> GetAsync(
        string userId,
        MonthPeriod period,
        CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            userId,
            period.Start,
            token => BuildAsync(userId, period, token),
            cancellationToken);

    private async Task<DashboardResponse> BuildAsync(
        string userId,
        MonthPeriod period,
        CancellationToken cancellationToken)
    {
        var transactions = await LoadTransactionsAsync(userId, period, cancellationToken);
        var contributions = await CalculateContributionsAsync(userId, period, cancellationToken);
        var accounts = await LoadAccountsAsync(userId, cancellationToken);
        var investments = await LoadInvestmentsAsync(userId, cancellationToken);

        var income = FinancialCalculator.CalculateIncome(transactions);
        var expenses = FinancialCalculator.CalculateExpenses(transactions);
        var netWorth = FinancialCalculator.CalculateNetWorth(accounts, investments);

        var previousNetWorth = await GetPreviousNetWorthAsync(userId, period.Start, netWorth, cancellationToken);
        var history = await GetHistoryAsync(userId, cancellationToken);
        var categoryImpact = CalculateCategoryImpact(transactions);

        return new DashboardResponse(
            period.Start,
            income,
            expenses,
            FinancialCalculator.CalculateBalance(transactions),
            contributions,
            FinancialCalculator.CalculateSavingsRate(contributions, income),
            netWorth,
            netWorth - previousNetWorth,
            categoryImpact,
            history);
    }

    private Task<List<Transaction>> LoadTransactionsAsync(
        string userId,
        MonthPeriod period,
        CancellationToken cancellationToken) =>
        database.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Where(transaction => transaction.UserId == userId)
            .Where(transaction => transaction.Date >= period.Start && transaction.Date < period.End)
            .ToListAsync(cancellationToken);

    private Task<List<Account>> LoadAccountsAsync(
        string userId,
        CancellationToken cancellationToken) =>
        database.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .ToListAsync(cancellationToken);

    private Task<List<Investment>> LoadInvestmentsAsync(
        string userId,
        CancellationToken cancellationToken) =>
        database.Investments
            .AsNoTracking()
            .Where(investment => investment.UserId == userId)
            .ToListAsync(cancellationToken);

    private async Task<decimal> CalculateContributionsAsync(
        string userId,
        MonthPeriod period,
        CancellationToken cancellationToken) =>
        await database.InvestmentContributions
            .AsNoTracking()
            .Where(contribution => contribution.Investment!.UserId == userId)
            .Where(contribution => contribution.Date >= period.Start && contribution.Date < period.End)
            .SumAsync(contribution => (decimal?)contribution.Amount, cancellationToken)
        ?? 0;

    private async Task<decimal> GetPreviousNetWorthAsync(
        string userId,
        DateOnly currentMonth,
        decimal currentNetWorth,
        CancellationToken cancellationToken)
    {
        var previousSnapshot = await database.MonthlySnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.UserId == userId && snapshot.Month < currentMonth)
            .OrderByDescending(snapshot => snapshot.Month)
            .FirstOrDefaultAsync(cancellationToken);

        return previousSnapshot?.NetWorth ?? currentNetWorth;
    }

    private Task<List<NetWorthHistoryResponse>> GetHistoryAsync(
        string userId,
        CancellationToken cancellationToken) =>
        database.MonthlySnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.UserId == userId)
            .OrderBy(snapshot => snapshot.Month)
            .Take(12)
            .Select(snapshot => new NetWorthHistoryResponse(snapshot.Month, snapshot.NetWorth))
            .ToListAsync(cancellationToken);

    private static List<CategoryImpactResponse> CalculateCategoryImpact(
        IEnumerable<Transaction> transactions) =>
        transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .GroupBy(transaction => new
            {
                transaction.CategoryId,
                Name = transaction.Category?.Name ?? "Uncategorized",
                Color = transaction.Category?.Color ?? "#94a3b8"
            })
            .Select(group => new CategoryImpactResponse(
                group.Key.CategoryId,
                group.Key.Name,
                group.Key.Color,
                group.Sum(transaction => transaction.Amount)))
            .OrderByDescending(category => category.Amount)
            .ToList();
}
