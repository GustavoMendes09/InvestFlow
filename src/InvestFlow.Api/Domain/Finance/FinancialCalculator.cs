using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Domain.Transactions;

namespace InvestFlow.Api.Domain.Finance;

public static class FinancialCalculator
{
    public static decimal CalculateIncome(IEnumerable<Transaction> transactions) =>
        transactions.Where(IsIncome).Sum(transaction => transaction.Amount);

    public static decimal CalculateExpenses(IEnumerable<Transaction> transactions) =>
        transactions.Where(IsExpense).Sum(transaction => transaction.Amount);

    public static IReadOnlyDictionary<Guid, decimal> CalculateExpensesByCategory(
        IEnumerable<Transaction> transactions) =>
        transactions
            .Where(IsExpense)
            .GroupBy(transaction => transaction.CategoryId ?? Guid.Empty)
            .ToDictionary(group => group.Key, group => group.Sum(transaction => transaction.Amount));

    public static decimal CalculateBalance(IEnumerable<Transaction> transactions) =>
        CalculateIncome(transactions) - CalculateExpenses(transactions);

    public static decimal CalculateSavingsRate(decimal investedAmount, decimal income) =>
        income == 0 ? 0 : Math.Round(investedAmount / income * 100, 1);

    public static decimal CalculateNetWorth(
        IEnumerable<Account> accounts,
        IEnumerable<Investment> investments)
    {
        var assets = accounts.Where(account => !account.IsDebt).Sum(account => account.Balance);
        var debts = accounts.Where(account => account.IsDebt).Sum(account => account.Balance);
        var investmentValue = investments.Sum(investment => investment.CurrentValue);

        return assets + investmentValue - debts;
    }

    public static decimal CalculateGoalProgress(decimal currentAmount, decimal targetAmount) =>
        targetAmount <= 0 ? 0 : Math.Round(currentAmount / targetAmount * 100, 1);

    public static decimal CalculateRemainingBudget(decimal budgetAmount, decimal expenses) =>
        budgetAmount - expenses;

    private static bool IsIncome(Transaction transaction) =>
        transaction.Type == TransactionType.Income;

    private static bool IsExpense(Transaction transaction) =>
        transaction.Type == TransactionType.Expense;
}
