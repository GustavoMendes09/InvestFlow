using System.Net.Http.Json;

namespace InvestFlow.Api.IntegrationTests.Infrastructure;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> ReadRequiredAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Unexpected HTTP {(int)response.StatusCode} ({response.StatusCode}). Body: {body}");
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new InvalidOperationException($"The response body could not be read as {typeof(T).Name}.");
    }
}
