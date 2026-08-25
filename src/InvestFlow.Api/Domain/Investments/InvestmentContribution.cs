namespace InvestFlow.Api.Domain.Investments;

public sealed class InvestmentContribution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvestmentId { get; set; }
    public Investment? Investment { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
