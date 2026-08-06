namespace EarnTrackerApi.Interfaces;

public interface IPayPalTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
