using FluentValidation;
using InvestFlow.Api.Domain.Budgets;
using InvestFlow.Api.Domain.Finance;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Budgets;

public static class BudgetEndpoints
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var budgets = endpoints.MapGroup("/budgets");

        budgets.MapGet("/", GetAllAsync);
        budgets.MapPut("/{categoryId:guid}", SaveAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        string? month,
        HttpContext context,
        AppDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        var period = MonthPeriodParser.ParseOrCurrent(month, timeProvider);

        var budgets = await database.Budgets
            .AsNoTracking()
            .Include(budget => budget.Category)
            .Where(budget => budget.UserId == userId && budget.Month == period.Start)
            .ToListAsync(cancellationToken);

        var spendingByCategory = await database.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Where(transaction => transaction.Date >= period.Start && transaction.Date < period.End)
            .Where(transaction => transaction.CategoryId.HasValue)
            .GroupBy(transaction => transaction.CategoryId!.Value)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Sum(transaction => transaction.Amount),
                cancellationToken);

        var response = budgets.Select(budget =>
        {
            var spent = spendingByCategory.GetValueOrDefault(budget.CategoryId);
            return new BudgetResponse(
                budget.Id,
                budget.CategoryId,
                budget.Category,
                budget.Month,
                budget.Amount,
                spent,
                FinancialCalculator.CalculateRemainingBudget(budget.Amount, spent));
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> SaveAsync(
        Guid categoryId,
        SaveBudgetRequest request,
        IValidator<SaveBudgetRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var userId = context.User.GetUserId();
        var month = new DateOnly(request.Month.Year, request.Month.Month, 1);
        var budget = await database.Budgets.FirstOrDefaultAsync(
            item => item.UserId == userId && item.CategoryId == categoryId && item.Month == month,
            cancellationToken);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = userId,
                CategoryId = categoryId,
                Month = month,
                Amount = request.Amount
            };
            database.Budgets.Add(budget);
        }
        else
        {
            budget.Amount = request.Amount;
        }

        await database.SaveChangesAsync(cancellationToken);
        return Results.Ok(budget);
    }
}
