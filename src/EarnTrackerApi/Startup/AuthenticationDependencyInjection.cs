using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EarnTrackerApi.Startup;

public static class AuthDI
{
    public static void AddJwt(this WebApplicationBuilder builder)
    {
        var issuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Auth authority not configured.");
        var audience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Auth audience not configured.");
        var key = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Auth key not configured.");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be at least 32 bytes long.");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key))
            };

            options.Authority = issuer;
            options.Audience = audience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });
    }
}
