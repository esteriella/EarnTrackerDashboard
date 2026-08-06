using System.Text;

namespace EarnTrackerApi.Helpers.Jwt;

public static class JwtSettingsHelper
{
    public static string Key { get; private set; } = string.Empty;
    public static string Issuer { get; private set; } = string.Empty;
    public static string Audience { get; private set; } = string.Empty;
    public static int MaxAge { get; private set; }
    public static int MaxRefreshAge { get; private set; }

    public static class CustomClaimTypes
    {
        public const string UserId = "user_id";
        public const string Email = "email";
        public const string DisplayName = "display_name";
    }

    public static JwtSettings GetJwtSettings(this IConfiguration configuration)
    {
        var settings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT configuration is missing.");

        Key = GetRequiredValue(settings.Key, "Jwt:Key");
        Issuer = GetRequiredValue(settings.Issuer, "Jwt:Issuer");
        Audience = GetRequiredValue(settings.Audience, "Jwt:Audience");
        MaxAge = settings.MaxAge;
        MaxRefreshAge = settings.MaxRefreshAge;

        if (Encoding.UTF8.GetByteCount(Key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes long.");
        }

        if (MaxAge is < 1 or > 1440)
        {
            throw new InvalidOperationException(
                "Jwt:MaxAge must be between 1 and 1440 minutes.");
        }

        if (MaxRefreshAge < MaxAge || MaxRefreshAge > 43200)
        {
            throw new InvalidOperationException(
                "Jwt:MaxRefreshAge must be at least MaxAge and no more than 43200 minutes.");
        }

        return new JwtSettings
        {
            Key = Key,
            Issuer = Issuer,
            Audience = Audience,
            MaxAge = MaxAge,
            MaxRefreshAge = MaxRefreshAge
        };
    }

    private static string GetRequiredValue(string? value, string key)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Configuration value '{key}' is required.")
            : value.Trim();
    }
}
