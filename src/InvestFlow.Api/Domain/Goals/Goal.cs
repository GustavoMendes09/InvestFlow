namespace InvestFlow.Api.Domain.Goals;

public sealed class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public GoalType Type { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly? Deadline { get; set; }
}
