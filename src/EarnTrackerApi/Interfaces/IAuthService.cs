using EarnTrackerApi.Dtos.AuthDto;

namespace EarnTrackerApi.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(
        RegisterDto request,
        CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(
        LoginDto request,
        CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshAsync(
        RefreshTokenDto request,
        CancellationToken cancellationToken = default);
}
