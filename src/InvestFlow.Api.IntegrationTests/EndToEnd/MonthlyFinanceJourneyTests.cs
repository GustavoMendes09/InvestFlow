using System.Net;
using System.Net.Http.Json;
using InvestFlow.Api.Domain.Accounts;
using InvestFlow.Api.Domain.Categories;
using InvestFlow.Api.Domain.Goals;
using InvestFlow.Api.Features.Accounts;
using InvestFlow.Api.Features.Budgets;
using InvestFlow.Api.Features.Dashboard;
using InvestFlow.Api.Features.Goals;
using InvestFlow.Api.Features.Investments;
using InvestFlow.Api.Features.Transactions;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.IntegrationTests.Infrastructure;

namespace InvestFlow.Api.IntegrationTests.EndToEnd;

[Collection(ApiTestCollection.Name)]
[Trait("Category", "EndToEnd")]
public sealed class MonthlyFinanceJourneyTests(ApiFixture fixture)
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(
        InvestFlowWebApplicationFactory.UtcNow.UtcDateTime);
    private static readonly string Month = $"{Today:yyyy-MM}";

    [Fact]
    public async Task UserCanCompleteTheMonthlyFinanceJourney()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var session = await TestUserSession.CreateAsync(fixture, cancellationToken);

        var categories = await session.Client.GetFromJsonAsync<List<Category>>(
            "/api/categories/",
            cancellationToken);
        Assert.NotNull(categories);
        Assert.Equal(5, categories.Count);
        var incomeCategory = Assert.Single(categories, category => category.IsIncome);
        var groceriesCategory = Assert.Single(categories, category => category.Name == "Groceries");

        var everyday = await PostAsync<Account>(
            session.Client,
            "/api/accounts/",
            new SaveAccountRequest("Everyday", 10_000, false),
            cancellationToken);
        await PostAsync<Account>(
            session.Client,
            "/api/accounts/",
            new SaveAccountRequest("Credit card", 1_500, true),
            cancellationToken);

        await PostAsync<Transaction>(
            session.Client,
            "/api/transactions/",
            new SaveTransactionRequest(
                TransactionType.Income,
                5_000,
                Today,
                incomeCategory.Id,
                "Salary",
                everyday.Id),
            cancellationToken);
        await PostAsync<Transaction>(
            session.Client,
            "/api/transactions/",
            new SaveTransactionRequest(
                TransactionType.Expense,
                1_200,
                Today,
                groceriesCategory.Id,
                "Groceries",
                everyday.Id),
            cancellationToken);

        using (var budgetResponse = await session.Client.PutAsJsonAsync(
                   $"/api/budgets/{groceriesCategory.Id}",
                   new SaveBudgetRequest(Today, 1_500),
                   cancellationToken))
        {
            Assert.Equal(HttpStatusCode.OK, budgetResponse.StatusCode);
        }

        var investment = await PostAsync<InvestmentResponseContract>(
            session.Client,
            "/api/investments/",
            new SaveInvestmentRequest("Index fund", "ETF", 2_000, 2_500, Today),
            cancellationToken);
        await PostAsync<ContributionResponse>(
            session.Client,
            $"/api/investments/{investment.Id}/contributions",
            new RecordContributionRequest(500, Today),
            cancellationToken);

        var goal = await PostAsync<GoalResponse>(
            session.Client,
            "/api/goals/",
            new SaveGoalRequest(
                "Emergency fund",
                GoalType.EmergencyFund,
                10_000,
                2_500,
                Today.AddYears(1)),
            cancellationToken);

        var budgets = await session.Client.GetFromJsonAsync<List<BudgetResponse>>(
            $"/api/budgets/?month={Month}",
            cancellationToken);
        var dashboard = await session.Client.GetFromJsonAsync<DashboardResponse>(
            $"/api/dashboard?month={Month}",
            cancellationToken);
        var investments = await session.Client.GetFromJsonAsync<List<InvestmentResponse>>(
            "/api/investments/",
            cancellationToken);

        var budget = Assert.Single(Assert.IsType<List<BudgetResponse>>(budgets));
        Assert.Equal(1_500, budget.Amount);
        Assert.Equal(1_200, budget.Spent);
        Assert.Equal(300, budget.Remaining);

        Assert.NotNull(dashboard);
        Assert.Equal(5_000, dashboard.Income);
        Assert.Equal(1_200, dashboard.Expenses);
        Assert.Equal(3_800, dashboard.Balance);
        Assert.Equal(500, dashboard.Invested);
        Assert.Equal(10, dashboard.SavingsRate);
        Assert.Equal(11_000, dashboard.NetWorth);
        Assert.Equal(0, dashboard.NetWorthVariation);
        var categoryImpact = Assert.Single(dashboard.CategoryImpact);
        Assert.Equal(groceriesCategory.Id, categoryImpact.CategoryId);
        Assert.Equal(1_200, categoryImpact.Amount);
        Assert.Contains(dashboard.History, snapshot => snapshot.Month == new DateOnly(2026, 8, 1)
            && snapshot.NetWorth == 11_000);

        var savedInvestment = Assert.Single(Assert.IsType<List<InvestmentResponse>>(investments));
        Assert.Equal(2_500, savedInvestment.InvestedAmount);
        Assert.Equal(2_500, savedInvestment.CurrentValue);
        Assert.Single(savedInvestment.Contributions);
        Assert.Equal(25, goal.Progress);
    }

    private static async Task<TResponse> PostAsync<TResponse>(
        HttpClient client,
        string path,
        object request,
        CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(path, request, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadRequiredAsync<TResponse>(cancellationToken);
    }

    private sealed record InvestmentResponseContract(Guid Id);
}
