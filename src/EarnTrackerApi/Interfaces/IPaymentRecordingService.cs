using System.Text.Json;

namespace EarnTrackerApi.Interfaces;

public interface IPaymentRecordingService
{
    Task RecordPayPalCapturesAsync(
        Guid userId,
        JsonElement response,
        CancellationToken cancellationToken = default);
}
