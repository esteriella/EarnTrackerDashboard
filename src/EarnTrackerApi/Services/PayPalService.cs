using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Utilities;

namespace EarnTrackerApi.Services;

public sealed class PayPalService(HttpClient client) : IPayPalService
{
    public async Task<JsonDocument> CreateOrderAsync(
        CreatePayPalOrderDto request,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = "default",
                    description = request.Description.Trim(),
                    amount = new
                    {
                        currency_code = request.Currency.Trim().ToUpperInvariant(),
                        value = request.Amount.ToString("0.00", CultureInfo.InvariantCulture)
                    }
                }
            }
        };

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "v2/checkout/orders")
        {
            Content = JsonContent.Create(payload)
        };
        AddRequestHeaders(message);

        using var response = await client.SendAsync(message, cancellationToken);
        return await HttpResponseReader.ReadAsync("PayPal", response, cancellationToken);
    }

    public async Task<JsonDocument> GetOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var id = EscapeId(orderId, nameof(orderId));
        using var response = await client.GetAsync(
            $"v2/checkout/orders/{id}",
            cancellationToken);
        return await HttpResponseReader.ReadAsync("PayPal", response, cancellationToken);
    }

    public async Task<JsonDocument> CaptureOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var id = EscapeId(orderId, nameof(orderId));
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"v2/checkout/orders/{id}/capture")
        {
            Content = JsonContent.Create(new { })
        };
        AddRequestHeaders(message);

        using var response = await client.SendAsync(message, cancellationToken);
        return await HttpResponseReader.ReadAsync("PayPal", response, cancellationToken);
    }

    public async Task<JsonDocument> GetCaptureAsync(
        string captureId,
        CancellationToken cancellationToken = default)
    {
        var id = EscapeId(captureId, nameof(captureId));
        using var response = await client.GetAsync(
            $"v2/payments/captures/{id}",
            cancellationToken);
        return await HttpResponseReader.ReadAsync("PayPal", response, cancellationToken);
    }

    private static string EscapeId(string id, string parameterName) =>
        Uri.EscapeDataString(Guard.Required(id, parameterName));

    private static void AddRequestHeaders(HttpRequestMessage message)
    {
        message.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString("N"));
        message.Headers.Add("Prefer", "return=representation");
    }
}
