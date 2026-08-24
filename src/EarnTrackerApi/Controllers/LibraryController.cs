using EarnTrackerApi.Dtos.LibraryDto;
using EarnTrackerApi.Extensions;
using EarnTrackerApi.Interfaces;
using EarnTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EarnTrackerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/library")]
public sealed class LibraryController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<LibraryOverviewResponse>> GetOverview(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var incomeSources = await unitOfWork.Library.GetIncomeSourcesAsync(
            userId,
            cancellationToken);
        var goals = await unitOfWork.Library.GetFinancialGoalsAsync(
            userId,
            cancellationToken);
        var transactions = incomeSources.SelectMany(source => source.Transactions).ToList();

        return Ok(new LibraryOverviewResponse(
            CreateTotals(transactions),
            incomeSources.Select(MapIncomeSource).ToList(),
            goals.Select(goal => MapGoal(goal, transactions)).ToList()));
    }

    /// <summary>Creates a financial goal for the signed-in user.</summary>
    [HttpPost("goals")]
    public async Task<ActionResult<FinancialGoalResponse>> CreateGoal(
        [FromBody] CreateFinancialGoalDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var goal = new FinancialGoal
        {
            UserId = userId,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            StartDate = request.StartDate,
            TargetDate = request.TargetDate
        };

        await unitOfWork.Library.AddFinancialGoalAsync(goal, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var incomeSources = await unitOfWork.Library.GetIncomeSourcesAsync(
            userId,
            cancellationToken);
        var transactions = incomeSources
            .SelectMany(source => source.Transactions)
            .ToList();

        return CreatedAtAction(
            nameof(GetOverview),
            MapGoal(goal, transactions));
    }

    private static IReadOnlyList<EarningsTotalResponse> CreateTotals(
        IEnumerable<EarningTransaction> transactions)
    {
        return transactions
            .Where(transaction => transaction.Status.Equals(
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(transaction => transaction.Currency.ToUpperInvariant())
            .Select(group => new EarningsTotalResponse(
                group.Key,
                group.Sum(transaction => transaction.Amount),
                group.Sum(transaction => transaction.Fee),
                group.Sum(transaction => transaction.Amount - transaction.Fee)))
            .OrderBy(total => total.Currency)
            .ToList();
    }

    private static IncomeSourceResponse MapIncomeSource(IncomeSource source)
    {
        return new IncomeSourceResponse(
            source.Id,
            source.Name,
            source.Provider,
            source.Currency,
            source.IsActive,
            source.Transactions
                .OrderByDescending(transaction => transaction.OccurredAt)
                .Select(transaction => new TransactionResponse(
                    transaction.Id,
                    transaction.ExternalId,
                    transaction.Amount,
                    transaction.Fee,
                    transaction.Currency,
                    transaction.Status,
                    transaction.Description,
                    transaction.OccurredAt))
                .ToList());
    }

    private static FinancialGoalResponse MapGoal(
        FinancialGoal goal,
        IEnumerable<EarningTransaction> transactions)
    {
        var currentAmount = transactions
            .Where(transaction =>
                transaction.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase) &&
                transaction.Currency.Equals(
                    goal.Currency,
                    StringComparison.OrdinalIgnoreCase))
            .Sum(transaction => transaction.Amount - transaction.Fee);
        var percentage = goal.TargetAmount <= 0
            ? 0
            : Math.Min(100, Math.Round(currentAmount / goal.TargetAmount * 100, 2));
        var isAchieved = currentAmount >= goal.TargetAmount;
        var status = isAchieved
            ? "Achieved"
            : goal.TargetDate < DateOnly.FromDateTime(DateTime.UtcNow)
                ? "Expired"
                : "Active";

        return new FinancialGoalResponse(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            currentAmount,
            percentage,
            goal.Currency,
            goal.StartDate,
            goal.TargetDate,
            status,
            isAchieved);
    }
}
