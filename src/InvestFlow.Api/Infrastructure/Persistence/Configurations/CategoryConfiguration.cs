using InvestFlow.Api.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestFlow.Api.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(category => new { category.UserId, category.Name }).IsUnique();
        builder.Property(category => category.Name).HasMaxLength(60);
        builder.Property(category => category.Color).HasMaxLength(7);
    }
}
