namespace InvestFlow.Api.Features.Dashboard;

public sealed record DashboardResponse(
    DateOnly Month,
    decimal Income,
    decimal Expenses,
    decimal Balance,
    decimal Invested,
    decimal SavingsRate,
    decimal NetWorth,
    decimal NetWorthVariation,
    IReadOnlyCollection<CategoryImpactResponse> CategoryImpact,
    IReadOnlyCollection<NetWorthHistoryResponse> History);
