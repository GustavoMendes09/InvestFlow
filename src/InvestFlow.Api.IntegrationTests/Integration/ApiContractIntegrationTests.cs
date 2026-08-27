using System.Net;
using System.Net.Http.Json;
using InvestFlow.Api.Features.Accounts;
using InvestFlow.Api.Features.Profile;
using InvestFlow.Api.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace InvestFlow.Api.IntegrationTests.Integration;

[Collection(ApiTestCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ApiContractIntegrationTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/api/accounts/")]
    [InlineData("/api/dashboard?month=2026-08")]
    [InlineData("/api/goals/")]
    public async Task ProtectedEndpoints_RejectUnauthenticatedRequests(string endpoint)
    {
        using var client = fixture.CreateClient();

        using var response = await client.GetAsync(endpoint, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CookieAuthentication_ProvidesTheSignedInProfile()
    {
        using var session = await TestUserSession.CreateAsync(
            fixture,
            TestContext.Current.CancellationToken);

        var profile = await session.Client.GetFromJsonAsync<ProfileResponse>(
            "/api/profile",
            TestContext.Current.CancellationToken);

        Assert.NotNull(profile);
        Assert.Equal(session.Email, profile.Email);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsValidationProblemDetails()
    {
        using var session = await TestUserSession.CreateAsync(
            fixture,
            TestContext.Current.CancellationToken);

        using var response = await session.Client.PostAsJsonAsync(
            "/api/accounts/",
            new SaveAccountRequest("", -1, false),
            TestContext.Current.CancellationToken);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains(nameof(SaveAccountRequest.Name), problem.Errors.Keys);
        Assert.Contains(nameof(SaveAccountRequest.Balance), problem.Errors.Keys);
    }
}
