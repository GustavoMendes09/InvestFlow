using InvestFlow.Api.Domain.Investments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.Property(investment => investment.Name).HasMaxLength(100);
        builder.Property(investment => investment.AssetClass).HasMaxLength(60);
        builder.Property(investment => investment.InvestedAmount).HasPrecision(18, 2);
        builder.Property(investment => investment.CurrentValue).HasPrecision(18, 2);
    }
}
