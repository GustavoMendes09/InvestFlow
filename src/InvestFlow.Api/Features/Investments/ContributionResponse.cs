namespace InvestFlow.Api.Features.Investments;

public sealed record ContributionResponse(Guid Id, decimal Amount, DateOnly Date);
