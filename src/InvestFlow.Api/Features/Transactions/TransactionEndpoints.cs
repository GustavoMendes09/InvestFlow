using FluentValidation;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Transactions;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var transactions = endpoints.MapGroup("/transactions");

        transactions.MapGet("/", GetAllAsync);
        transactions.MapPost("/", CreateAsync);
        transactions.MapPut("/{id:guid}", UpdateAsync);
        transactions.MapDelete("/{id:guid}", DeleteAsync);

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

        var transactions = await database.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.Category)
            .Include(transaction => transaction.Account)
            .Where(transaction => transaction.UserId == userId)
            .Where(transaction => transaction.Date >= period.Start && transaction.Date < period.End)
            .OrderByDescending(transaction => transaction.Date)
            .ToListAsync(cancellationToken);

        return Results.Ok(transactions);
    }

    private static async Task<IResult> CreateAsync(
        SaveTransactionRequest request,
        IValidator<SaveTransactionRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var transaction = new Transaction { UserId = context.User.GetUserId() };
        ApplyRequest(transaction, request);

        database.Transactions.Add(transaction);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/transactions/{transaction.Id}", transaction);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveTransactionRequest request,
        IValidator<SaveTransactionRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var transaction = await FindOwnedTransactionAsync(
            id,
            context.User.GetUserId(),
            database,
            cancellationToken);

        if (transaction is null)
        {
            return Results.NotFound();
        }

        ApplyRequest(transaction, request);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Ok(transaction);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var transaction = await FindOwnedTransactionAsync(
            id,
            context.User.GetUserId(),
            database,
            cancellationToken);

        if (transaction is null)
        {
            return Results.NotFound();
        }

        database.Transactions.Remove(transaction);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static void ApplyRequest(Transaction transaction, SaveTransactionRequest request)
    {
        transaction.Type = request.Type;
        transaction.Amount = request.Amount;
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;
        transaction.Description = request.Description?.Trim();
        transaction.AccountId = request.AccountId;
    }

    private static Task<Transaction?> FindOwnedTransactionAsync(
        Guid id,
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken) =>
        database.Transactions.FirstOrDefaultAsync(
            transaction => transaction.Id == id && transaction.UserId == userId,
            cancellationToken);
}
