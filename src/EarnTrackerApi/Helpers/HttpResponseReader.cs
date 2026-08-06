using System.Text.Json;
using EarnTrackerApi.Exceptions;

namespace EarnTrackerApi.Helpers;

public static class HttpResponseReader
{
    public static async Task<JsonDocument> ReadAsync(
        string provider,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException(
                provider,
                $"HTTP {(int)response.StatusCode}: {content}");
        }

        try
        {
            return JsonDocument.Parse(content);
        }
        catch (JsonException exception)
        {
            throw new ExternalServiceException(
                provider,
                $"The provider returned invalid JSON: {exception.Message}");
        }
    }
}
