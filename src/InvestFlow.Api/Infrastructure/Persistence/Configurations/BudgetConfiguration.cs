using InvestFlow.Api.Domain.Budgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasIndex(budget => new { budget.UserId, budget.CategoryId, budget.Month }).IsUnique();
        builder.Property(budget => budget.Amount).HasPrecision(18, 2);
    }
}
