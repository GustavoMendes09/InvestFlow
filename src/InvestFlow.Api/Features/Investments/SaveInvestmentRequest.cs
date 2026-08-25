namespace InvestFlow.Api.Features.Investments;

public sealed record SaveInvestmentRequest(
    string Name,
    string AssetClass,
    decimal InvestedAmount,
    decimal CurrentValue,
    DateOnly UpdatedAt);
