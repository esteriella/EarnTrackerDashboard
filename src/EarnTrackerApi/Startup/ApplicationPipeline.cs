using EarnTrackerApi.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EarnTrackerApi.Startup;

public static class ApplicationPipeline
{
    public static async Task UseApplicationPipelineAsync(this WebApplication app)
    {
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
        var database = scope.ServiceProvider.GetRequiredService<EarnTrackerDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseStartup");

        await database.Database.EnsureCreatedAsync();
        logger.LogInformation("EarnTracker PostgreSQL database is ready");
    }
}
