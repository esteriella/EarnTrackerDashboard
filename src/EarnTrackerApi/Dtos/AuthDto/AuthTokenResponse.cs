namespace EarnTrackerApi.Dtos.AuthDto;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string TokenType = "Bearer");
