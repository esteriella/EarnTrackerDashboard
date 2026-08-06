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
    Task AddIncomeSourceAsync(
        IncomeSource source,
        CancellationToken cancellationToken = default);
    Task AddFinancialGoalAsync(
        FinancialGoal goal,
        CancellationToken cancellationToken = default);
}
