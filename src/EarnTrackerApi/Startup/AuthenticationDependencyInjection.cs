using EarnTrackerApi.Helpers.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EarnTrackerApi.Startup;

public static class AuthDI
{
    public static void AddJwt(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration.GetJwtSettings();
        builder.Services.AddSingleton(settings);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                JwtConfiguration.CreateValidationParameters(settings);
            options.Audience = settings.Audience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });
    }
}
