using EarnTrackerApi.Data;
using Microsoft.EntityFrameworkCore;

namespace EarnTrackerApi.Startup;

public static class Database
{
    public static void ConfigureDatabase(
        this WebApplicationBuilder builder,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                postgresqlOptions =>
                {
                    postgresqlOptions.UseQuerySplittingBehavior(
                        QuerySplittingBehavior.SplitQuery);
                    postgresqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                    postgresqlOptions.CommandTimeout(60);
                });
        }, ServiceLifetime.Scoped);
    }
}
