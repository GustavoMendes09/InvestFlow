using InvestFlow.Api.Features.Accounts;
using InvestFlow.Api.Features.Budgets;
using InvestFlow.Api.Features.Categories;
using InvestFlow.Api.Features.Dashboard;
using InvestFlow.Api.Features.Goals;
using InvestFlow.Api.Features.Investments;
using InvestFlow.Api.Features.Profile;
using InvestFlow.Api.Features.Transactions;

namespace InvestFlow.Api.Features;

public static class EndpointRegistration
{
    public static IEndpointRouteBuilder MapInvestFlowFeatures(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api").RequireAuthorization();

        api.MapAccountEndpoints();
        api.MapBudgetEndpoints();
        api.MapCategoryEndpoints();
        api.MapDashboardEndpoints();
        api.MapGoalEndpoints();
        api.MapInvestmentEndpoints();
        api.MapProfileEndpoints();
        api.MapTransactionEndpoints();

        return endpoints;
    }
}
