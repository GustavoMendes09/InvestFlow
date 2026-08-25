using InvestFlow.Api.Domain.Goals;

namespace InvestFlow.Api.Features.Goals;

public sealed record GoalResponse(
    Guid Id,
    string Name,
    GoalType Type,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateOnly? Deadline,
    decimal Progress);
