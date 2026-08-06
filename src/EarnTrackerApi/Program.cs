using EarnTrackerApi.Startup;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

Logging.Main(builder);
builder.AddJwt();
builder.ConfigureDatabase(connectionString);
builder.AddExternalHttpClients();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

await app.UseApplicationPipelineAsync();

app.Run();

public partial class Program;
