using InvestFlow.Api.Domain.Categories;

namespace InvestFlow.Api.Domain.Budgets;

public sealed class Budget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateOnly Month { get; set; }
    public decimal Amount { get; set; }
}
