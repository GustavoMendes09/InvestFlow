using System.Net.Http.Json;

namespace InvestFlow.Api.IntegrationTests.Infrastructure;

public sealed record TestUserSession(string Email, HttpClient Client) : IDisposable
{
    private const string Password = "StrongPass1";

    public static async Task<TestUserSession> CreateAsync(
        ApiFixture fixture,
        CancellationToken cancellationToken = default)
    {
        var client = fixture.CreateClient();
        var email = $"test-{Guid.NewGuid():N}@investflow.local";

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password = Password },
            cancellationToken);
        await EnsureSuccessAsync(registerResponse, "register test user", cancellationToken);

        using var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login?useCookies=true",
            new { email, password = Password },
            cancellationToken);
        await EnsureSuccessAsync(loginResponse, "sign in test user", cancellationToken);

        return new TestUserSession(email, client);
    }

    public void Dispose() => Client.Dispose();

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Could not {operation}. Status: {(int)response.StatusCode}. Body: {body}");
    }
}
