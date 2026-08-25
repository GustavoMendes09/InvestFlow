using InvestFlow.Api.Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.Property(goal => goal.Name).HasMaxLength(100);
        builder.Property(goal => goal.TargetAmount).HasPrecision(18, 2);
        builder.Property(goal => goal.CurrentAmount).HasPrecision(18, 2);
    }
}
