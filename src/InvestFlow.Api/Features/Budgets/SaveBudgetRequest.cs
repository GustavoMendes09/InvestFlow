namespace InvestFlow.Api.Features.Budgets;

public sealed record SaveBudgetRequest(DateOnly Month, decimal Amount);
