using EarnTrackerApi.Interfaces;

namespace EarnTrackerApi.Data.UnitOfWork;

public sealed class UnitOfWork(
    AppDbContext context,
    IAuthRepository auth,
    ILibraryRepository library) : IUnitOfWork
{
    public IAuthRepository Auth { get; } = auth;
    public ILibraryRepository Library { get; } = library;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
