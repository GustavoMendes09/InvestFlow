namespace InvestFlow.Api.Domain.Categories;

public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public bool IsIncome { get; set; }
}
