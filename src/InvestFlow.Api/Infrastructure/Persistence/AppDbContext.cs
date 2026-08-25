using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Budgets;
using InvestFlow.Api.Domain.Categories;
using InvestFlow.Api.Domain.Goals;
using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Domain.Snapshots;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<InvestmentContribution> InvestmentContributions => Set<InvestmentContribution>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<MonthlySnapshot> MonthlySnapshots => Set<MonthlySnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
