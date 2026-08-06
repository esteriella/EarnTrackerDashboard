using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EarnTrackerApi.Dtos.AuthDto;
using EarnTrackerApi.Helpers.Jwt;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;

namespace EarnTrackerApi.Services;

public sealed class JwtService(JwtSettings settings, TimeProvider clock)
    : IJwtService
{
    public AuthTokenResponse CreateToken(User user)
    {
        var expiresAt = clock.GetUtcNow().AddMinutes(settings.MaxAge);

        var claims = new[]
        {
            new Claim(JwtSettingsHelper.CustomClaimTypes.UserId, user.Id.ToString()),
            new Claim(JwtSettingsHelper.CustomClaimTypes.Email, user.Email),
            new Claim(JwtSettingsHelper.CustomClaimTypes.DisplayName, user.DisplayName)
        };

        var credentials = JwtConfiguration.CreateSigningCredentials(settings);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
