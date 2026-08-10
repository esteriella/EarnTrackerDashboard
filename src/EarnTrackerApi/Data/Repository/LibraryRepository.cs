using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EarnTrackerApi.Data.Repository;

public sealed class LibraryRepository(AppDbContext context) : ILibraryRepository
{
    public async Task<IReadOnlyList<IncomeSource>> GetIncomeSourcesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.IncomeSources
            .AsNoTracking()
            .Where(source => source.UserId == userId)
            .Include(source => source.Transactions)
            .OrderBy(source => source.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialGoal>> GetFinancialGoalsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.FinancialGoals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId)
            .OrderBy(goal => goal.TargetDate)
            .ToListAsync(cancellationToken);
    }

    public Task<IncomeSource?> GetIncomeSourceAsync(
        Guid userId,
        string provider,
        string currency,
        CancellationToken cancellationToken = default)
    {
        return context.IncomeSources.SingleOrDefaultAsync(
            source => source.UserId == userId &&
                source.Provider == provider &&
                source.Currency == currency,
            cancellationToken);
    }

    public Task<EarningTransaction?> GetTransactionAsync(
        Guid incomeSourceId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return context.Transactions.SingleOrDefaultAsync(
            transaction => transaction.IncomeSourceId == incomeSourceId &&
                transaction.ExternalId == externalId,
            cancellationToken);
    }

    public Task AddIncomeSourceAsync(
        IncomeSource source,
        CancellationToken cancellationToken = default)
    {
        return context.IncomeSources.AddAsync(source, cancellationToken).AsTask();
    }

    public Task AddTransactionAsync(
        EarningTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        return context.Transactions.AddAsync(transaction, cancellationToken).AsTask();
    }

    public Task AddFinancialGoalAsync(
        FinancialGoal goal,
        CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals.AddAsync(goal, cancellationToken).AsTask();
    }
}
