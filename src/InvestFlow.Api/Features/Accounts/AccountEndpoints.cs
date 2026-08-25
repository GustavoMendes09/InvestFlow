using FluentValidation;
using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Accounts;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accounts = endpoints.MapGroup("/accounts");

        accounts.MapGet("/", GetAllAsync);
        accounts.MapPost("/", CreateAsync);
        accounts.MapPut("/{id:guid}", UpdateAsync);
        accounts.MapDelete("/{id:guid}", DeleteAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        var accounts = await database.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .OrderBy(account => account.IsDebt)
            .ThenBy(account => account.Name)
            .ToListAsync(cancellationToken);

        return Results.Ok(accounts);
    }

    private static async Task<IResult> CreateAsync(
        SaveAccountRequest request,
        IValidator<SaveAccountRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var account = new Account
        {
            UserId = context.User.GetUserId(),
            Name = request.Name.Trim(),
            Balance = request.Balance,
            IsDebt = request.IsDebt
        };

        database.Accounts.Add(account);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/accounts/{account.Id}", account);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveAccountRequest request,
        IValidator<SaveAccountRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var account = await FindOwnedAccountAsync(id, context.User.GetUserId(), database, cancellationToken);
        if (account is null)
        {
            return Results.NotFound();
        }

        account.Name = request.Name.Trim();
        account.Balance = request.Balance;
        account.IsDebt = request.IsDebt;

        await database.SaveChangesAsync(cancellationToken);
        return Results.Ok(account);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var account = await FindOwnedAccountAsync(id, context.User.GetUserId(), database, cancellationToken);
        if (account is null)
        {
            return Results.NotFound();
        }

        database.Accounts.Remove(account);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static Task<Account?> FindOwnedAccountAsync(
        Guid id,
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken) =>
        database.Accounts.FirstOrDefaultAsync(
            account => account.Id == id && account.UserId == userId,
            cancellationToken);
}
