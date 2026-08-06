using System.Text.Json;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Utilities;

namespace EarnTrackerApi.Services;

public sealed class PayPalService(HttpClient client) : IPayPalService
{
    public async Task<JsonDocument> GetCaptureAsync(
        string captureId,
        CancellationToken cancellationToken = default)
    {
        var id = Uri.EscapeDataString(Guard.Required(captureId, nameof(captureId)));
        using var response = await client.GetAsync(
            $"v2/payments/captures/{id}",
            cancellationToken);
        return await HttpResponseReader.ReadAsync("PayPal", response, cancellationToken);
    }
}
