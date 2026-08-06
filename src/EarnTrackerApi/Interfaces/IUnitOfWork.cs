namespace EarnTrackerApi.Interfaces;

public interface IUnitOfWork
{
    IAuthRepository Auth { get; }
    ILibraryRepository Library { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
