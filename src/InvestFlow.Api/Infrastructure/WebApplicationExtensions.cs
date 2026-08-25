using InvestFlow.Api.Infrastructure.Persistence;
using InvestFlow.Api.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Infrastructure;

public static class WebApplicationExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication application)
    {
        if (!application.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = application.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await database.Database.MigrateAsync();

        var identitySeeder = scope.ServiceProvider.GetRequiredService<DevelopmentIdentitySeeder>();
        await identitySeeder.SeedAsync();
    }
}
