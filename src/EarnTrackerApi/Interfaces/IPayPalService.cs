using System.Text.Json;

namespace EarnTrackerApi.Interfaces;

public interface IPayPalService
{
    Task<JsonDocument> GetCaptureAsync(
        string captureId,
        CancellationToken cancellationToken = default);
}
