using InvestFlow.Api.Domain.Users;
using InvestFlow.Api.Features;
using InvestFlow.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInvestFlowServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitialiseDatabaseAsync();

app.MapGroup("/api/auth").MapIdentityApi<AppUser>();
app.MapPost("/api/auth/logout", SignOutAsync).RequireAuthorization();
app.MapInvestFlowFeatures();
app.MapGet("/", (IConfiguration configuration) =>
    Results.Redirect(configuration["ClientOrigin"] ?? "http://localhost:5173"));
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

static async Task<IResult> SignOutAsync(SignInManager<AppUser> signInManager)
{
    await signInManager.SignOutAsync();
    return Results.NoContent();
}

public partial class Program;
