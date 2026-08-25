using InvestFlow.Api.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace InvestFlow.Api.Infrastructure.Seeding;

public sealed class DevelopmentIdentitySeeder(
    UserManager<AppUser> userManager,
    IOptions<DevelopmentSeedOptions> seedOptions,
    ILogger<DevelopmentIdentitySeeder> logger)
{
    public async Task SeedAsync()
    {
        var options = seedOptions.Value;
        if (!options.Enabled || await userManager.FindByNameAsync(options.UserName) is not null)
        {
            return;
        }

        var user = new AppUser
        {
            UserName = options.UserName,
            Email = options.Email,
            EmailConfirmed = true
        };

        // The initial development password is intentionally allowed to be shorter
        // than the password policy. Normal user registrations still require 8 characters.
        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, options.Password);
        var createResult = await userManager.CreateAsync(user);
        EnsureSucceeded(createResult, "create the development user");

        logger.LogInformation(
            "Created development seed user {UserName} with email {Email}.",
            options.UserName,
            options.Email);
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Could not {action}: {errors}");
    }
}
