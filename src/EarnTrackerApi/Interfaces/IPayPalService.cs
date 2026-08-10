using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;

namespace EarnTrackerApi.Interfaces;

public interface IPayPalService
{
    Task<JsonDocument> CreateOrderAsync(
        CreatePayPalOrderDto request,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> GetOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> CaptureOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> GetCaptureAsync(
        string captureId,
        CancellationToken cancellationToken = default);
}
