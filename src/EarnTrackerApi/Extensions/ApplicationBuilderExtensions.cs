using EarnTrackerApi.Middleware;

namespace EarnTrackerApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApplicationExceptionHandling(
        this IApplicationBuilder application)
    {
        return application.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
