using System.Net.Http.Headers;
using System.Text;
using EarnTrackerApi.Exceptions;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Startup;

namespace EarnTrackerApi.Services;

public sealed class PayPalTokenService(
    IHttpClientFactory clientFactory,
    IConfiguration configuration,
    ICacheService cache) : IPayPalTokenService
{
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return cache.GetOrCreateAsync(
            CacheKeys.PayPalAccessToken,
            RequestAccessTokenAsync,
            TimeSpan.FromMinutes(8),
            cancellationToken);
    }

    private async Task<string> RequestAccessTokenAsync(CancellationToken cancellationToken)
    {
        var clientId = configuration["PayPal:ClientId"]
            ?? throw new InvalidOperationException("PayPal client ID is not configured.");
        var clientSecret = configuration["PayPal:ClientSecret"]
            ?? throw new InvalidOperationException("PayPal client secret is not configured.");
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var client = clientFactory.CreateClient(HttpClientDI.PayPalAccountsClient);
        using var response = await client.SendAsync(request, cancellationToken);
        using var document = await HttpResponseReader.ReadAsync(
            "PayPal",
            response,
            cancellationToken);

        return document.RootElement.TryGetProperty("access_token", out var token)
            ? token.GetString() ?? throw new ExternalServiceException(
                "PayPal",
                "The access token was empty.")
            : throw new ExternalServiceException(
                "PayPal",
                "The access token was missing.");
    }
}
