namespace EarnTrackerApi.Dtos.AuthDto;

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAt);
