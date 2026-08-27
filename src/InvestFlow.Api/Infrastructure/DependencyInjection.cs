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
        services.AddInvestFlowCaching(configuration);
        services.AddIdentity();
        services.AddInvestFlowDataProtection(configuration, environment);
        services.AddAuthorization();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<DashboardService>();
        services.AddScoped<NetWorthSnapshotService>();
        services.AddScoped<DevelopmentIdentitySeeder>();
        services.Configure<DevelopmentSeedOptions>(
            configuration.GetSection(DevelopmentSeedOptions.SectionName));
        services.AddInvestFlowCors(configuration);

        return services;
    }

    private static void AddInvestFlowDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var dataProtection = services.AddDataProtection();
        var configuredKeyPath = configuration["DataProtection:KeyPath"];
        if (string.IsNullOrWhiteSpace(configuredKeyPath) && !environment.IsDevelopment())
        {
            return;
        }

        var keyPath = string.IsNullOrWhiteSpace(configuredKeyPath)
            ? Path.Combine(environment.ContentRootPath, ".data-protection-keys")
            : configuredKeyPath;
        var keyDirectory = new DirectoryInfo(keyPath);
        dataProtection.PersistKeysToFileSystem(keyDirectory);
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static void AddInvestFlowCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "InvestFlow:";
            });
        }

        services.Configure<DashboardCacheOptions>(
            configuration.GetSection(DashboardCacheOptions.SectionName));
        services.AddSingleton<DashboardCache>();
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
