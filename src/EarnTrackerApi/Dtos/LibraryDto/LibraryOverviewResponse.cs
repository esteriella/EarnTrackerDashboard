namespace EarnTrackerApi.Dtos.LibraryDto;

public sealed record LibraryOverviewResponse(
    IReadOnlyList<EarningsTotalResponse> Totals,
    IReadOnlyList<IncomeSourceResponse> IncomeSources,
    IReadOnlyList<FinancialGoalResponse> FinancialGoals);

public sealed record EarningsTotalResponse(
    string Currency,
    decimal Gross,
    decimal Fees,
    decimal Net);

public sealed record IncomeSourceResponse(
    Guid Id,
    string Name,
    string Provider,
    string Currency,
    bool IsActive,
    IReadOnlyList<TransactionResponse> Transactions);

public sealed record TransactionResponse(
    Guid Id,
    string ExternalId,
    decimal Amount,
    decimal Fee,
    string Currency,
    string Status,
    string? Description,
    DateTimeOffset OccurredAt);

public sealed record FinancialGoalResponse(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage,
    string Currency,
    DateOnly StartDate,
    DateOnly TargetDate);
