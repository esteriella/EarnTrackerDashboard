using EarnTrackerApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

namespace EarnTrackerApi.Startup;

public static class ApplicationStartup
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Freelancer Earnings Tracker API";
                document.Info.Description =
                    "API for authentication, earnings, financial goals, " +
                    "PayPal, Paystack, and cryptocurrency integrations.";

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Paste the access token returned by login or registration."
                    };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();
                var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();

                if (requiresAuthorization && !allowsAnonymous)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", null)] = []
                    });
                }

                return Task.CompletedTask;
            });
        });
        services.AddProblemDetails();
        services.AddAuthorization();
        services.AddApplicationCore();

        services.AddApplicationCors(configuration);

        return services;
    }

    private static IServiceCollection AddApplicationCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }
}
