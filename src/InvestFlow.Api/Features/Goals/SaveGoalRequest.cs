using InvestFlow.Api.Domain.Goals;

namespace InvestFlow.Api.Features.Goals;

public sealed record SaveGoalRequest(
    string Name,
    GoalType Type,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateOnly? Deadline);
