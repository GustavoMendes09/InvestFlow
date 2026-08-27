namespace InvestFlow.Api.Features.Dashboard;

public sealed class DashboardCacheOptions
{
    public const string SectionName = "Caching:Dashboard";

    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
}
