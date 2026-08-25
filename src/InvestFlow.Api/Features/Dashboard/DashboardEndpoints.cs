using InvestFlow.Api.Features.Common;

namespace InvestFlow.Api.Features.Dashboard;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/dashboard", GetAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        string? month,
        HttpContext context,
        DashboardService dashboard,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var period = MonthPeriodParser.ParseOrCurrent(month, timeProvider);
        var response = await dashboard.GetAsync(
            context.User.GetUserId(),
            period,
            cancellationToken);

        return Results.Ok(response);
    }
}
