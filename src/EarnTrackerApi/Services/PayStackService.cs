using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Utilities;

namespace EarnTrackerApi.Services;

public sealed class PayStackService(HttpClient client) : IPayStackService
{
    public async Task<JsonDocument> InitializeTransactionAsync(
        Guid userId,
        CreatePayStackTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var reference = $"ET-{userId:N}-{Guid.NewGuid():N}";
        var amountInSubunits = decimal.Round(
            request.Amount * 100m,
            0,
            MidpointRounding.AwayFromZero);
        var payload = new
        {
            email = request.Email.Trim(),
            amount = amountInSubunits.ToString("0", CultureInfo.InvariantCulture),
            currency = request.Currency.Trim().ToUpperInvariant(),
            reference,
            callback_url = string.IsNullOrWhiteSpace(request.CallbackUrl)
                ? null
                : request.CallbackUrl.Trim(),
            metadata = new
            {
                earntracker_user_id = userId,
                description = request.Description.Trim()
            }
        };

        using var response = await client.PostAsJsonAsync(
            "transaction/initialize",
            payload,
            cancellationToken);
        return await HttpResponseReader.ReadAsync(
            "Paystack",
            response,
            cancellationToken);
    }

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
