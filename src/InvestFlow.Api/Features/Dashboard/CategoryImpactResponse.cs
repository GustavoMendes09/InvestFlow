namespace InvestFlow.Api.Features.Dashboard;

public sealed record CategoryImpactResponse(
    Guid? CategoryId,
    string Name,
    string Color,
    decimal Amount);
