using EarnTrackerApi.Dtos.AuthDto;
using EarnTrackerApi.Exceptions;
using EarnTrackerApi.Helpers;
using EarnTrackerApi.Helpers.Jwt;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;

namespace EarnTrackerApi.Services;

public sealed class AuthService(
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    JwtSettings jwtSettings,
    TimeProvider clock) : IAuthService
{
    public async Task<LoginResponseDto> RegisterAsync(
        RegisterDto request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var existingUser = await unitOfWork.Auth.GetByEmailAsync(
            email,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            DisplayName = request.Name.Trim(),
            PasswordHash = HashHelper.Hash(request.Password)
        };

        await unitOfWork.Auth.AddAsync(user, cancellationToken);
        return await CreateResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Auth.GetByEmailAsync(
            NormalizeEmail(request.Email),
            cancellationToken);

        if (user is null || !HashHelper.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return await CreateResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResponseDto> RefreshAsync(
        RefreshTokenDto request,
        CancellationToken cancellationToken = default)
    {
        var storedToken = await unitOfWork.Auth.GetRefreshTokenAsync(
            HashHelper.HashToken(request.RefreshToken),
            cancellationToken);
        var now = clock.GetUtcNow();

        if (storedToken?.User is null || storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= now)
        {
            throw new InvalidCredentialsException();
        }

        storedToken.RevokedAt = now;
        return await CreateResponseAsync(storedToken.User, cancellationToken);
    }

    private async Task<LoginResponseDto> CreateResponseAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var accessToken = jwtService.CreateToken(user);
        var rawRefreshToken = CodeGenerator.GenerateSecureToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashHelper.HashToken(rawRefreshToken),
            ExpiresAt = clock.GetUtcNow().AddMinutes(jwtSettings.MaxRefreshAge)
        };

        await unitOfWork.Auth.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            Tag = user.Id.ToString(),
            Name = user.DisplayName,
            Token = accessToken.AccessToken,
            RefreshToken = rawRefreshToken
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
