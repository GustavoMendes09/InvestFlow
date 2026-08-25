using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Categories;

namespace InvestFlow.Api.Domain.Transactions;

public sealed class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
}
