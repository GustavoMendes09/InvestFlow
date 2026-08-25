using InvestFlow.Api.Domain.Categories;

namespace InvestFlow.Api.Features.Budgets;

public sealed record BudgetResponse(
    Guid Id,
    Guid CategoryId,
    Category? Category,
    DateOnly Month,
    decimal Amount,
    decimal Spent,
    decimal Remaining);
