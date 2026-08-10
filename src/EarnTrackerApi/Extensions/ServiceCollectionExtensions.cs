using EarnTrackerApi.Data.Repository;
using EarnTrackerApi.Data.UnitOfWork;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Services;

namespace EarnTrackerApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaymentRecordingService, PaymentRecordingService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
