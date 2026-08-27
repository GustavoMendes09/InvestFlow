using System.Net;
using System.Net.Http.Json;
using InvestFlow.Api.Domain.Categories;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Features.Transactions;
using InvestFlow.Api.IntegrationTests.Infrastructure;

namespace InvestFlow.Api.IntegrationTests.EndToEnd;

[Collection(ApiTestCollection.Name)]
[Trait("Category", "EndToEnd")]
public sealed class TransactionCrudEndToEndTests(ApiFixture fixture)
{
    [Fact]
    public async Task TransactionCanBeCreatedMovedToAnotherMonthAndDeleted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var session = await TestUserSession.CreateAsync(fixture, cancellationToken);
        var categories = await session.Client.GetFromJsonAsync<List<Category>>(
            "/api/categories/",
            cancellationToken);
        var groceries = Assert.Single(
            Assert.IsType<List<Category>>(categories),
            category => category.Name == "Groceries");
        var august = new DateOnly(2026, 8, 15);
        var september = new DateOnly(2026, 9, 2);

        using var createResponse = await session.Client.PostAsJsonAsync(
            "/api/transactions/",
            new SaveTransactionRequest(
                TransactionType.Expense,
                75,
                august,
                groceries.Id,
                "  Weekly groceries  ",
                null),
            cancellationToken);
        var transaction = await createResponse.ReadRequiredAsync<Transaction>(cancellationToken);

        using var updateResponse = await session.Client.PutAsJsonAsync(
            $"/api/transactions/{transaction.Id}",
            new SaveTransactionRequest(
                TransactionType.Expense,
                90,
                september,
                groceries.Id,
                "Updated groceries",
                null),
            cancellationToken);
        var updated = await updateResponse.ReadRequiredAsync<Transaction>(cancellationToken);
        var augustTransactions = await session.Client.GetFromJsonAsync<List<Transaction>>(
            "/api/transactions/?month=2026-08",
            cancellationToken);
        var septemberTransactions = await session.Client.GetFromJsonAsync<List<Transaction>>(
            "/api/transactions/?month=2026-09",
            cancellationToken);

        using var deleteResponse = await session.Client.DeleteAsync(
            $"/api/transactions/{transaction.Id}",
            cancellationToken);
        var afterDelete = await session.Client.GetFromJsonAsync<List<Transaction>>(
            "/api/transactions/?month=2026-09",
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal("Weekly groceries", transaction.Description);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(90, updated.Amount);
        Assert.Equal(september, updated.Date);
        Assert.Empty(Assert.IsType<List<Transaction>>(augustTransactions));
        Assert.Contains(
            Assert.IsType<List<Transaction>>(septemberTransactions),
            item => item.Id == transaction.Id);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Empty(Assert.IsType<List<Transaction>>(afterDelete));
    }
}
