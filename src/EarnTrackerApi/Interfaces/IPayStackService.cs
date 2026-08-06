using System.Text.Json;

namespace EarnTrackerApi.Interfaces;

public interface IPayStackService
{
    Task<JsonDocument> VerifyTransactionAsync(
        string reference,
        CancellationToken cancellationToken = default);
}
