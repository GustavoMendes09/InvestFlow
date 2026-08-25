using FluentValidation;
using InvestFlow.Api.Domain.Finance;
using InvestFlow.Api.Domain.Goals;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Goals;

public static class GoalEndpoints
{
    public static IEndpointRouteBuilder MapGoalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var goals = endpoints.MapGroup("/goals");

        goals.MapGet("/", GetAllAsync);
        goals.MapPost("/", CreateAsync);
        goals.MapPut("/{id:guid}", UpdateAsync);
        goals.MapDelete("/{id:guid}", DeleteAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        var goals = await database.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId)
            .OrderBy(goal => goal.Deadline)
            .ToListAsync(cancellationToken);

        var response = goals.Select(ToResponse);
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateAsync(
        SaveGoalRequest request,
        IValidator<SaveGoalRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var goal = new Goal
        {
            UserId = context.User.GetUserId(),
            Name = request.Name.Trim(),
            Type = request.Type,
            TargetAmount = request.TargetAmount,
            CurrentAmount = request.CurrentAmount,
            Deadline = request.Deadline
        };

        database.Goals.Add(goal);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/goals/{goal.Id}", ToResponse(goal));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveGoalRequest request,
        IValidator<SaveGoalRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var goal = await FindOwnedGoalAsync(
            id,
            context.User.GetUserId(),
            database,
            cancellationToken);

        if (goal is null)
        {
            return Results.NotFound();
        }

        ApplyRequest(goal, request);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToResponse(goal));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var goal = await FindOwnedGoalAsync(
            id,
            context.User.GetUserId(),
            database,
            cancellationToken);

        if (goal is null)
        {
            return Results.NotFound();
        }

        database.Goals.Remove(goal);
        await database.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static void ApplyRequest(Goal goal, SaveGoalRequest request)
    {
        goal.Name = request.Name.Trim();
        goal.Type = request.Type;
        goal.TargetAmount = request.TargetAmount;
        goal.CurrentAmount = request.CurrentAmount;
        goal.Deadline = request.Deadline;
    }

    private static GoalResponse ToResponse(Goal goal) =>
        new(
            goal.Id,
            goal.Name,
            goal.Type,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.Deadline,
            FinancialCalculator.CalculateGoalProgress(goal.CurrentAmount, goal.TargetAmount));

    private static Task<Goal?> FindOwnedGoalAsync(
        Guid id,
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken) =>
        database.Goals.FirstOrDefaultAsync(
            goal => goal.Id == id && goal.UserId == userId,
            cancellationToken);
}
