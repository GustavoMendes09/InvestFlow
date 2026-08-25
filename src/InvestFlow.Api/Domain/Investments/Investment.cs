namespace InvestFlow.Api.Domain.Investments;

public sealed class Investment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required string AssetClass { get; set; }
    public decimal InvestedAmount { get; set; }
    public decimal CurrentValue { get; set; }
    public DateOnly UpdatedAt { get; set; }
    public List<InvestmentContribution> Contributions { get; set; } = [];
}
