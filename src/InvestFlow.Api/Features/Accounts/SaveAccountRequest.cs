namespace InvestFlow.Api.Features.Accounts;

public sealed record SaveAccountRequest(string Name, decimal Balance, bool IsDebt);
