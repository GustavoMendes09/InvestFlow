using System.Net;
using System.Net.Http.Json;
using InvestFlow.Api.Domain.Goals;
using InvestFlow.Api.Features.Goals;
using InvestFlow.Api.IntegrationTests.Infrastructure;

namespace InvestFlow.Api.IntegrationTests.EndToEnd;

[Collection(ApiTestCollection.Name)]
[Trait("Category", "EndToEnd")]
public sealed class UserIsolationEndToEndTests(ApiFixture fixture)
{
    [Fact]
    public async Task UsersCannotReadUpdateOrDeleteAnotherUsersGoal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var owner = await TestUserSession.CreateAsync(fixture, cancellationToken);
        using var otherUser = await TestUserSession.CreateAsync(fixture, cancellationToken);
        using var createResponse = await owner.Client.PostAsJsonAsync(
            "/api/goals/",
            new SaveGoalRequest("Travel", GoalType.Travel, 5_000, 500, null),
            cancellationToken);
        var goal = await createResponse.ReadRequiredAsync<GoalResponse>(cancellationToken);

        var otherUsersGoals = await otherUser.Client.GetFromJsonAsync<List<GoalResponse>>(
            "/api/goals/",
            cancellationToken);
        using var updateResponse = await otherUser.Client.PutAsJsonAsync(
            $"/api/goals/{goal.Id}",
            new SaveGoalRequest("Changed", GoalType.Other, 1_000, 100, null),
            cancellationToken);
        using var deleteResponse = await otherUser.Client.DeleteAsync(
            $"/api/goals/{goal.Id}",
            cancellationToken);
        var ownersGoals = await owner.Client.GetFromJsonAsync<List<GoalResponse>>(
            "/api/goals/",
            cancellationToken);

        Assert.Empty(Assert.IsType<List<GoalResponse>>(otherUsersGoals));
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Contains(
            Assert.IsType<List<GoalResponse>>(ownersGoals),
            item => item.Id == goal.Id && item.Name == "Travel");
    }
}
