using FluentValidation;
using InvestFlow.Api.Domain.Categories;
using InvestFlow.Api.Features.Common;
using InvestFlow.Api.Features.Dashboard;
using InvestFlow.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Features.Categories;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var categories = endpoints.MapGroup("/categories");

        categories.MapGet("/", GetAllAsync);
        categories.MapPost("/", CreateAsync);
        categories.MapPut("/{id:guid}", UpdateAsync);
        categories.MapDelete("/{id:guid}", DeleteAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        await CreateDefaultCategoriesWhenEmptyAsync(userId, database, cancellationToken);

        var categories = await database.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .OrderByDescending(category => category.IsIncome)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return Results.Ok(categories);
    }

    private static async Task<IResult> CreateAsync(
        SaveCategoryRequest request,
        IValidator<SaveCategoryRequest> validator,
        HttpContext context,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var category = new Category
        {
            UserId = context.User.GetUserId(),
            Name = request.Name.Trim(),
            Color = request.Color,
            IsIncome = request.IsIncome
        };

        database.Categories.Add(category);
        await database.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/categories/{category.Id}", category);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveCategoryRequest request,
        IValidator<SaveCategoryRequest> validator,
        HttpContext context,
        AppDbContext database,
        DashboardCache dashboardCache,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateRequestAsync(request, cancellationToken) is { } validationProblem)
        {
            return validationProblem;
        }

        var category = await FindOwnedCategoryAsync(id, context.User.GetUserId(), database, cancellationToken);
        if (category is null)
        {
            return Results.NotFound();
        }

        category.Name = request.Name.Trim();
        category.Color = request.Color;
        category.IsIncome = request.IsIncome;

        await database.SaveChangesAsync(cancellationToken);
        await dashboardCache.InvalidateAsync(category.UserId, cancellationToken);
        return Results.Ok(category);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        HttpContext context,
        AppDbContext database,
        DashboardCache dashboardCache,
        CancellationToken cancellationToken)
    {
        var category = await FindOwnedCategoryAsync(id, context.User.GetUserId(), database, cancellationToken);
        if (category is null)
        {
            return Results.NotFound();
        }

        database.Categories.Remove(category);
        await database.SaveChangesAsync(cancellationToken);
        await dashboardCache.InvalidateAsync(category.UserId, cancellationToken);

        return Results.NoContent();
    }

    private static async Task CreateDefaultCategoriesWhenEmptyAsync(
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken)
    {
        if (await database.Categories.AnyAsync(category => category.UserId == userId, cancellationToken))
        {
            return;
        }

        database.Categories.AddRange(
            CreateCategory(userId, "Income", "#2563eb", true),
            CreateCategory(userId, "Housing", "#7c3aed"),
            CreateCategory(userId, "Groceries", "#16a34a"),
            CreateCategory(userId, "Transport", "#ea580c"),
            CreateCategory(userId, "Lifestyle", "#db2777"));

        await database.SaveChangesAsync(cancellationToken);
    }

    private static Category CreateCategory(
        string userId,
        string name,
        string color,
        bool isIncome = false) =>
        new()
        {
            UserId = userId,
            Name = name,
            Color = color,
            IsIncome = isIncome
        };

    private static Task<Category?> FindOwnedCategoryAsync(
        Guid id,
        string userId,
        AppDbContext database,
        CancellationToken cancellationToken) =>
        database.Categories.FirstOrDefaultAsync(
            category => category.Id == id && category.UserId == userId,
            cancellationToken);
}
