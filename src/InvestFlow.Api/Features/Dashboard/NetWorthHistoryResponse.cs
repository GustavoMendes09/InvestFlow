namespace InvestFlow.Api.Features.Dashboard;

public sealed record NetWorthHistoryResponse(DateOnly Month, decimal NetWorth);
