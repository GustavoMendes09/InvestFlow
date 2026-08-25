namespace InvestFlow.Api.Domain.Snapshots;

public sealed class MonthlySnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public DateOnly Month { get; set; }
    public decimal NetWorth { get; set; }
}
