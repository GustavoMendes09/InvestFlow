using FluentValidation;
using InvestFlow.Api.Domain.Investments;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Features.Dashboard;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Investments;

public static class InvestmentEndpoints
{
    public static IEndpointRouteBuilder MapInvestmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var investments = endpoints.MapGroup("/investments");

        investments.MapGet("/", GetAllAsync);
        investments.MapPost("/", CreateAsync);
        investments.MapPut("/{id:guid}", UpdateAsync);
        investments.MapPost("/{id:guid}/contributions", RecordContributionAsync);
        investments.MapDelete("/{id:guid}", DeleteAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        var investments = await database.Investments
            .AsNoTracking()
            .Where(investment => investment.UserId == userId)
            .OrderBy(investment => investment.Name)
            .Select(investment => new InvestmentResponse(
                investment.Id,
                investment.Name,
                investment.AssetClass,
                investment.InvestedAmount,
                investment.CurrentValue,
                investment.UpdatedAt,
                investment.Contributions
                    .OrderByDescending(contribution => contribution.Date)
                    .Select(contribution => new ContributionResponse(
                        contribution.Id,
                        contribution.Amount,
                        contribution.Date))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Results.Ok(investments);
    }

    private static async Task<IResult> CreateAsync(
        SaveInvestmentRequest request,
        IValidator<SaveInvestmentRequest> validator,
        HttpContext context,
        AppDbContext database,
        NetWorthSnapshotService snapshotService,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var userId = context.User.GetUserId();
        var investment = new Investment
        {
            UserId = userId,
            Name = request.Name.Trim(),
            AssetClass = request.AssetClass.Trim(),
            InvestedAmount = request.InvestedAmount,
            CurrentValue = request.CurrentValue,
            UpdatedAt = request.UpdatedAt
        };

        database.Investments.Add(investment);
        await database.SaveChangesAsync(cancellationToken);
        await snapshotService.UpdateCurrentAsync(userId, cancellationToken);

        return Results.Created($"/api/investments/{investment.Id}", investment);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveInvestmentRequest request,
        IValidator<SaveInvestmentRequest> validator,
        HttpContext context,
        AppDbContext database,
        NetWorthSnapshotService snapshotService,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var userId = context.User.GetUserId();
        var investment = await FindOwnedInvestmentAsync(
            id,
            userId,
            database,
            cancellationToken);

        if (investment is null)
        {
            return Results.NotFound();
        }

        ApplyRequest(investment, request);
        await database.SaveChangesAsync(cancellationToken);
        await snapshotService.UpdateCurrentAsync(userId, cancellationToken);

        return Results.Ok(investment);
    }

    private static async Task<IResult> RecordContributionAsync(
        Guid id,
        RecordContributionRequest request,
        IValidator<RecordContributionRequest> validator,
        HttpContext context,
        AppDbContext database,
        DashboardCache dashboardCache,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var investment = await FindOwnedInvestmentAsync(
            id,
            context.User.GetUserId(),
            database,
            cancellationToken);

        if (investment is null)
        {
            return Results.NotFound();
        }

        var contribution = new InvestmentContribution
        {
            InvestmentId = investment.Id,
            Amount = request.Amount,
            Date = request.Date
        };

        investment.InvestedAmount += request.Amount;
        database.InvestmentContributions.Add(contribution);
        await database.SaveChangesAsync(cancellationToken);
        await dashboardCache.InvalidateAsync(investment.UserId, cancellationToken);

        return Results.Created(
            $"/api/investments/{id}/contributions/{contribution.Id}",
            new ContributionResponse(contribution.Id, contribution.Amount, contribution.Date));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        HttpContext context,
        AppDbContext database,
        NetWorthSnapshotService snapshotService,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        var investment = await FindOwnedInvestmentAsync(
            id,
            userId,
            database,
            cancellationToken);

        if (investment is null)
        {
            return Results.NotFound();
        }

        database.Investments.Remove(investment);
        await database.SaveChangesAsync(cancellationToken);
        await snapshotService.UpdateCurrentAsync(userId, cancellationToken);

        return Results.NoContent();
    }

    private static void ApplyRequest(Investment investment, SaveInvestmentRequest request)
    {
        investment.Name = request.Name.Trim();
        investment.AssetClass = request.AssetClass.Trim();
        investment.InvestedAmount = request.InvestedAmount;
        investment.CurrentValue = request.CurrentValue;
        investment.UpdatedAt = request.UpdatedAt;
    }

    private static Task<Investment?> FindOwnedInvestmentAsync(
        Guid id,
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken) =>
        database.Investments.FirstOrDefaultAsync(
            investment => investment.Id == id && investment.UserId == userId,
            cancellationToken);
}
