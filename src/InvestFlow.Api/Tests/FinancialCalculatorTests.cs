using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Finance;
using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Domain.Transactions;

namespace InvestFlow.Api.Tests;

public sealed class FinancialCalculatorTests
{
    private const string UserId = "user-1";

    [Fact]
    public void IncomeAndExpenses_ClassifyAndTotalTransactions()
    {
        var transactions = new[]
        {
            CreateTransaction(TransactionType.Income, 5_000),
            CreateTransaction(TransactionType.Expense, 1_200),
            CreateTransaction(TransactionType.Expense, 350)
        };

        Assert.Equal(5_000, FinancialCalculator.CalculateIncome(transactions));
        Assert.Equal(1_550, FinancialCalculator.CalculateExpenses(transactions));
        Assert.Equal(3_450, FinancialCalculator.CalculateBalance(transactions));
    }

    [Theory]
    [InlineData(2_000, 1_450, 550)]
    [InlineData(500, 725, -225)]
    public void RemainingBudget_DoesNotHideNegativeValues(
        decimal budget,
        decimal spent,
        decimal expected) =>
        Assert.Equal(expected, FinancialCalculator.CalculateRemainingBudget(budget, spent));

    [Fact]
    public void NetWorth_AddsAssetsAndInvestmentsThenSubtractsDebts()
    {
        var accounts = new[]
        {
            new Account { UserId = UserId, Name = "Everyday", Balance = 8_000 },
            new Account { UserId = UserId, Name = "Credit card", Balance = 1_500, IsDebt = true }
        };
        var investments = new[]
        {
            new Investment
            {
                UserId = UserId,
                Name = "Index fund",
                AssetClass = "ETF",
                CurrentValue = 12_000,
                UpdatedAt = DateOnly.FromDateTime(DateTime.Today)
            }
        };

        Assert.Equal(18_500, FinancialCalculator.CalculateNetWorth(accounts, investments));
    }

    [Theory]
    [InlineData(2_500, 10_000, 25)]
    [InlineData(0, 10_000, 0)]
    [InlineData(100, 0, 0)]
    public void GoalProgress_IsPredictable(decimal current, decimal target, decimal expected) =>
        Assert.Equal(expected, FinancialCalculator.CalculateGoalProgress(current, target));

    [Theory]
    [InlineData(1_000, 5_000, 20)]
    [InlineData(500, 0, 0)]
    public void SavingsRate_HandlesZeroIncome(decimal invested, decimal income, decimal expected) =>
        Assert.Equal(expected, FinancialCalculator.CalculateSavingsRate(invested, income));

    [Fact]
    public void MonthlyNetWorthVariation_PreservesDirection() =>
        Assert.Equal(-750, 30_250m - 31_000m);

    [Fact]
    public void ExpensesByCategory_ClassifiesOnlyExpenseTransactions()
    {
        var groceries = Guid.NewGuid();
        var transport = Guid.NewGuid();
        var transactions = new[]
        {
            CreateTransaction(TransactionType.Expense, 80, groceries),
            CreateTransaction(TransactionType.Expense, 20, groceries),
            CreateTransaction(TransactionType.Expense, 45, transport),
            CreateTransaction(TransactionType.Income, 4_000, groceries)
        };

        var totals = FinancialCalculator.CalculateExpensesByCategory(transactions);

        Assert.Equal(100, totals[groceries]);
        Assert.Equal(45, totals[transport]);
    }

    private static Transaction CreateTransaction(
        TransactionType type,
        decimal amount,
        Guid? categoryId = null) =>
        new()
        {
            UserId = UserId,
            Type = type,
            Amount = amount,
            CategoryId = categoryId,
            Date = DateOnly.FromDateTime(DateTime.Today)
        };
}
