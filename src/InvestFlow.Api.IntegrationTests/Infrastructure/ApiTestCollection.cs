namespace InvestFlow.Api.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiTestCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "InvestFlow API with PostgreSQL";
}
