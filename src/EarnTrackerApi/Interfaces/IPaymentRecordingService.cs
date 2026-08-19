using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;
using EarnTrackerApi.Models;

namespace EarnTrackerApi.Interfaces;

public interface IPaymentRecordingService
{
    Task<EarningTransaction> RecordDemoPaymentAsync(
        Guid userId,
        CreateDemoPaymentDto request,
        CancellationToken cancellationToken = default);

    Task RecordPayPalCapturesAsync(
        Guid userId,
        JsonElement response,
        CancellationToken cancellationToken = default);
}
