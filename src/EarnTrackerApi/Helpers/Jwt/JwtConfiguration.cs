using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EarnTrackerApi.Helpers.Jwt;

public static class JwtConfiguration
{
    public static TokenValidationParameters CreateValidationParameters(
        JwtSettings settings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer
                ?? throw new InvalidOperationException("JWT issuer is missing."),
            ValidAudience = settings.Audience
                ?? throw new InvalidOperationException("JWT audience is missing."),
            IssuerSigningKey = CreateSecurityKey(settings),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public static SigningCredentials CreateSigningCredentials(JwtSettings settings)
    {
        return new SigningCredentials(
            CreateSecurityKey(settings),
            SecurityAlgorithms.HmacSha256);
    }

    private static SymmetricSecurityKey CreateSecurityKey(JwtSettings settings)
    {
        var key = settings.Key
            ?? throw new InvalidOperationException("JWT signing key is missing.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
