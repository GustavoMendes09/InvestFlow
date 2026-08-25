using InvestFlow.Api.Domain.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class MonthlySnapshotConfiguration : IEntityTypeConfiguration<MonthlySnapshot>
{
    public void Configure(EntityTypeBuilder<MonthlySnapshot> builder)
    {
        builder.HasIndex(snapshot => new { snapshot.UserId, snapshot.Month }).IsUnique();
        builder.Property(snapshot => snapshot.NetWorth).HasPrecision(18, 2);
    }
}
