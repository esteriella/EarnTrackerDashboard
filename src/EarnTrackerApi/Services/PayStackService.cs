using System.Text.Json;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Utilities;

namespace EarnTrackerApi.Services;

public sealed class PayStackService(HttpClient client) : IPayStackService
{
    public async Task<JsonDocument> VerifyTransactionAsync(
        string reference,
        CancellationToken cancellationToken = default)
    {
        var value = Uri.EscapeDataString(Guard.Required(reference, nameof(reference)));
        using var response = await client.GetAsync(
            $"transaction/verify/{value}",
            cancellationToken);
        return await HttpResponseReader.ReadAsync("Paystack", response, cancellationToken);
    }
}
