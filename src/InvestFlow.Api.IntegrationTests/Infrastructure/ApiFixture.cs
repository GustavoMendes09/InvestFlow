using Testcontainers.PostgreSql;

namespace InvestFlow.Api.IntegrationTests.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:18.6-alpine")
        .WithDatabase("investflow_tests")
        .WithUsername("investflow")
        .WithPassword("investflow_tests")
        .Build();

    public InvestFlowWebApplicationFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await database.StartAsync(TestContext.Current.CancellationToken);
        Factory = new InvestFlowWebApplicationFactory(database.GetConnectionString());

        using var client = CreateClient();
        using var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public HttpClient CreateClient() =>
        Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    public async ValueTask DisposeAsync()
    {
        Factory?.Dispose();
        await database.DisposeAsync();
    }
}
