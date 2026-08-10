using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Services;
using System.Net.Http.Headers;

namespace EarnTrackerApi.Startup;

public static class HttpClientDI
{
    public const string PayPalClient = "PayPal";
    public const string PayPalAccountsClient = "PayPalAccounts";
    public const string PayStackClient = "PayStack";
    public const string CryptoClient = "Crypto";

    public static void AddExternalHttpClients(this WebApplicationBuilder builder)
    {
        _ = GetRequiredSetting(builder, "PayPal:ClientId");
        _ = GetRequiredSetting(builder, "PayPal:ClientSecret");
        var payStackClientSecret = GetRequiredSetting(builder, "PayStack:ClientSecret");

        var payPalBaseAddress = GetRequiredUri(builder, "PayPal:BaseUrl");
        EnsureSafePayPalEnvironment(builder, payPalBaseAddress);
        builder.Services.AddHttpClient(PayPalAccountsClient, client =>
            ConfigureClient(client, payPalBaseAddress));
        builder.Services.AddSingleton<IPayPalTokenService, PayPalTokenService>();
        builder.Services.AddTransient<PayPalAuthenticationHandler>();
        builder.Services.AddHttpClient<IPayPalService, PayPalService>(client =>
            ConfigureClient(client, payPalBaseAddress))
            .AddHttpMessageHandler<PayPalAuthenticationHandler>();

        AddClient(
            builder,
            CryptoClient,
            "ExternalServices:Crypto");

        var payStackBaseAddress = GetRequiredUri(builder, "PayStack:BaseUrl");
        builder.Services.AddTransient<PayStackAuthenticationHandler>();
        builder.Services.AddHttpClient<IPayStackService, PayStackService>(client =>
            ConfigureClient(client, payStackBaseAddress))
            .AddHttpMessageHandler<PayStackAuthenticationHandler>();

        // PayPal credentials are applied only when requesting an OAuth token.
        _ = payStackClientSecret;
    }

    private static void EnsureSafePayPalEnvironment(
        WebApplicationBuilder builder,
        Uri baseAddress)
    {
        const string sandboxHost = "api-m.sandbox.paypal.com";

        if (builder.Environment.IsDevelopment() &&
            !string.Equals(
                baseAddress.Host,
                sandboxHost,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Development must use the PayPal Sandbox API at " +
                $"https://{sandboxHost}/. The configured PayPal:BaseUrl " +
                $"points to '{baseAddress.Host}'.");
        }
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
