using EarnTrackerApi.Helpers;
using EarnTrackerApi.Startup;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(renderPort, out var port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

HashHelperSettings.Configure(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
}

var configuredDatabase = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DATABASE_URL or connection string 'DefaultConnection' is required.");
var connectionString = PostgresConnectionString.Normalize(configuredDatabase);

Logging.Main(builder);
builder.AddJwt();
builder.ConfigureDatabase(connectionString);
builder.AddExternalHttpClients();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

await app.UseApplicationPipelineAsync();

app.Run();

public partial class Program;
