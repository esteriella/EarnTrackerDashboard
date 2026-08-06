namespace EarnTrackerApi.Dtos.AuthDto;

public sealed record LoginResponseDto
{
    public string Tag { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
