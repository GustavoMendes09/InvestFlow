using InvestFlow.Api.Domain.Investments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class InvestmentContributionConfiguration : IEntityTypeConfiguration<InvestmentContribution>
{
    public void Configure(EntityTypeBuilder<InvestmentContribution> builder) =>
        builder.Property(contribution => contribution.Amount).HasPrecision(18, 2);
}
