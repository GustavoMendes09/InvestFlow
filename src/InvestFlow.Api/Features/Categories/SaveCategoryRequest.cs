namespace InvestFlow.Api.Features.Categories;

public sealed record SaveCategoryRequest(string Name, string Color, bool IsIncome);
