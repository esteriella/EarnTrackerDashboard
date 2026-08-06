using System.Net.Http.Headers;

namespace EarnTrackerApi.Startup;

public static class HttpClientDI
{
    public const string PayPalClient = "PayPal";
    public const string PayStackClient = "PayStack";
    public const string CryptoClient = "Crypto";

    public static void AddExternalHttpClients(this WebApplicationBuilder builder)
    {
        _ = GetRequiredSetting(builder, "PayPal:ClientId");
        _ = GetRequiredSetting(builder, "PayPal:ClientSecret");
        var payStackClientSecret = GetRequiredSetting(builder, "PayStack:ClientSecret");

        AddClient(builder, PayPalClient, "PayPal:BaseUrl");
        AddClient(
            builder,
            CryptoClient,
            "ExternalServices:Crypto");

        var payStackBaseAddress = GetRequiredUri(builder, "PayStack:BaseUrl");
        builder.Services.AddHttpClient(PayStackClient, client =>
        {
            ConfigureClient(client, payStackBaseAddress);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", payStackClientSecret);
        });

        // PayPal credentials are applied only when requesting an OAuth token.
    }

    private static void AddClient(
        WebApplicationBuilder builder,
        string clientName,
        string configurationKey)
    {
        var baseAddress = GetRequiredUri(builder, configurationKey);

        builder.Services.AddHttpClient(clientName, client =>
            ConfigureClient(client, baseAddress));
    }

    private static Uri GetRequiredUri(
        WebApplicationBuilder builder,
        string configurationKey)
    {
        var value = builder.Configuration[configurationKey];

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException(
                $"{configurationKey} is required and must be a valid absolute URL.");
        }

        return uri;
    }

    private static string GetRequiredSetting(
        WebApplicationBuilder builder,
        string configurationKey)
    {
        var value = builder.Configuration[configurationKey];

        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{configurationKey} is required.")
            : value;
    }

    private static void ConfigureClient(HttpClient client, Uri baseAddress)
    {
        client.BaseAddress = baseAddress;
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("EarnTrackerApi/1.0");
    }
}
