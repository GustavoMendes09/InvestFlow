using FluentValidation;
using InvestFlow.Api.Domain.Users;
using InvestFlow.Api.Features.Dashboard;
using InvestFlow.Api.Infrastructure.Persistence;
using InvestFlow.Api.Infrastructure.Seeding;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvestFlow.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInvestFlowServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDatabase(configuration);
        services.AddIdentity();
        services.AddInvestFlowDataProtection(environment);
        services.AddAuthorization();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<DashboardService>();
        services.AddScoped<DevelopmentIdentitySeeder>();
        services.Configure<DevelopmentSeedOptions>(
            configuration.GetSection(DevelopmentSeedOptions.SectionName));
        services.AddInvestFlowCors(configuration);

        return services;
    }

    private static void AddInvestFlowDataProtection(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var dataProtection = services.AddDataProtection();
        if (!environment.IsDevelopment())
        {
            return;
        }

        var keyDirectory = new DirectoryInfo(
            Path.Combine(environment.ContentRootPath, ".data-protection-keys"));
        dataProtection.PersistKeysToFileSystem(keyDirectory);
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static void AddIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityApiEndpoints<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();
    }

    private static void AddInvestFlowCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var clientOrigin = configuration["ClientOrigin"] ?? "http://localhost:5173";

        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy
                .WithOrigins(clientOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
    }
}
