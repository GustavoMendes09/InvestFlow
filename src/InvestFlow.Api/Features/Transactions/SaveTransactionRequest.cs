using InvestFlow.Api.Domain.Transactions;

namespace InvestFlow.Api.Features.Transactions;

public sealed record SaveTransactionRequest(
    TransactionType Type,
    decimal Amount,
    DateOnly Date,
    Guid? CategoryId,
    string? Description,
    Guid? AccountId);
