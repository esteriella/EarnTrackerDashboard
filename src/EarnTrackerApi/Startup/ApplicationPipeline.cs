using EarnTrackerApi.Data;
using EarnTrackerApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EarnTrackerApi.Startup;

public static class ApplicationPipeline
{
    public static async Task UseApplicationPipelineAsync(this WebApplication app)
    {
        app.UseApplicationExceptionHandling();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        else
        {
            app.UseExceptionHandler();
        }

        await InitializeDatabaseAsync(app);

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            service = "EarnTrackerApi",
            timestamp = DateTimeOffset.UtcNow
        })).AllowAnonymous();

        app.MapControllers();
    }

    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseStartup");

        await database.Database.EnsureCreatedAsync();
        await database.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE "Users"
            ADD COLUMN IF NOT EXISTS "PasswordHash" character varying(500)
            NOT NULL DEFAULT '';
            """);
        await database.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "RefreshTokens" (
                "Id" uuid PRIMARY KEY,
                "UserId" uuid NOT NULL REFERENCES "Users" ("Id") ON DELETE CASCADE,
                "TokenHash" character varying(64) NOT NULL,
                "ExpiresAt" timestamp with time zone NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "RevokedAt" timestamp with time zone NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_RefreshTokens_TokenHash"
            ON "RefreshTokens" ("TokenHash");
            """);
        logger.LogInformation("EarnTracker PostgreSQL database is ready");
    }
}
