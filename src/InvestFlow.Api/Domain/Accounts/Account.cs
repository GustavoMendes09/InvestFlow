namespace InvestFlow.Api.Domain.Accounts;

public sealed class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public decimal Balance { get; set; }
    public bool IsDebt { get; set; }
}
