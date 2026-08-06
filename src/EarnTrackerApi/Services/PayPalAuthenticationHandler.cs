using System.Net.Http.Headers;
using EarnTrackerApi.Interfaces;

namespace EarnTrackerApi.Services;

public sealed class PayPalAuthenticationHandler(IPayPalTokenService tokenService)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenService.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
