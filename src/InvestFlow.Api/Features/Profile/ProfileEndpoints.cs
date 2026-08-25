namespace InvestFlow.Api.Features.Profile;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/profile", GetProfile);
        return endpoints;
    }

    private static IResult GetProfile(HttpContext context) =>
        Results.Ok(new ProfileResponse(context.User.Identity?.Name));
}
