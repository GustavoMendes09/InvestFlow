namespace InvestFlow.Api.Features.Investments;

public sealed record InvestmentResponse(
    Guid Id,
    string Name,
    string AssetClass,
    decimal InvestedAmount,
    decimal CurrentValue,
    DateOnly UpdatedAt,
    IReadOnlyCollection<ContributionResponse> Contributions);
