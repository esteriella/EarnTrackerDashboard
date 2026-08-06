using System.Net.Http.Headers;

namespace EarnTrackerApi.Services;

public sealed class PayStackAuthenticationHandler(IConfiguration configuration)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var secret = configuration["PayStack:ClientSecret"]
            ?? throw new InvalidOperationException("Paystack client secret is not configured.");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
        return base.SendAsync(request, cancellationToken);
    }
}
