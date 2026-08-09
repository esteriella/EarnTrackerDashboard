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

        await database.Database.MigrateAsync();
        logger.LogInformation(
            "EarnTracker PostgreSQL database migrations are up to date");
    }
}
