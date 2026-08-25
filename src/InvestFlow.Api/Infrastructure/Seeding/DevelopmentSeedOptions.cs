namespace InvestFlow.Api.Infrastructure.Seeding;

public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "DevelopmentSeed";

    public bool Enabled { get; init; } = true;
    public string UserName { get; init; } = "admin";
    public string Email { get; init; } = "admin@investflow.local";
    public string Password { get; init; } = "123";
}
