using System.Text.Json;
using EarnTrackerApi.Dtos.IntegrationDto;

namespace EarnTrackerApi.Interfaces;

public interface IPayStackService
{
    Task<JsonDocument> InitializeTransactionAsync(
        Guid userId,
        CreatePayStackTransactionDto request,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> VerifyTransactionAsync(
        string reference,
        CancellationToken cancellationToken = default);
}
