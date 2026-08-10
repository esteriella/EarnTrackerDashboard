using EarnTrackerApi.Models;

namespace EarnTrackerApi.Interfaces;

public interface ILibraryRepository
{
    Task<IReadOnlyList<IncomeSource>> GetIncomeSourcesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FinancialGoal>> GetFinancialGoalsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<IncomeSource?> GetIncomeSourceAsync(
        Guid userId,
        string provider,
        string currency,
        CancellationToken cancellationToken = default);
    Task<EarningTransaction?> GetTransactionAsync(
        Guid incomeSourceId,
        string externalId,
        CancellationToken cancellationToken = default);
    Task AddIncomeSourceAsync(
        IncomeSource source,
        CancellationToken cancellationToken = default);
    Task AddTransactionAsync(
        EarningTransaction transaction,
        CancellationToken cancellationToken = default);
    Task AddFinancialGoalAsync(
        FinancialGoal goal,
        CancellationToken cancellationToken = default);
}
