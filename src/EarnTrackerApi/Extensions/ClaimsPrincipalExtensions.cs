using EarnTrackerApi.Helpers.Jwt;
using System.Security.Claims;

namespace EarnTrackerApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(
            JwtSettingsHelper.CustomClaimTypes.UserId);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("The access token has no valid user ID.");
    }
}
