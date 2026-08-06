using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EarnTrackerApi.Data.Repository;

public sealed class AuthRepository(AppDbContext context) : IAuthRepository
{
    public Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return context.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return context.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);
    }

    public Task AddRefreshTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        return context.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }
}
