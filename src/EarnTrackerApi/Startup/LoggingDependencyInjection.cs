using Serilog;
using Serilog.Events;
using SerilogTracing;
using System.Globalization;

namespace EarnTrackerApi.Startup;

public static class Logging
{
    private const string ServiceName = "EarnTracker API";
    private const string FileOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
        "{Message:lj}{NewLine}{Exception}";

#pragma warning disable CS7022 // Required name; Program.cs uses top-level statements.
    public static void Main(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override(
                "Microsoft.AspNetCore.Hosting",
                LogEventLevel.Warning)
            .MinimumLevel.Override(
                "Microsoft.AspNetCore.Routing",
                LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", ServiceName)
            .Enrich.WithProperty(
                "Environment",
                builder.Environment.EnvironmentName)
            .WriteTo.Console()
            .WriteTo.File(
                "logs/api-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: FileOutputTemplate,
                formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", ServiceName)
                .Enrich.WithProperty(
                    "Environment",
                    context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/api-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: FileOutputTemplate,
                    formatProvider: CultureInfo.InvariantCulture);
        });

        // Activity tracing must be attached after Serilog.
        if (builder.Environment.IsDevelopment())
        {
            new ActivityListenerConfiguration().TraceToSharedLogger();
        }
    }
#pragma warning restore CS7022
}
