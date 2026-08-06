namespace EarnTrackerApi.Models;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<IncomeSource> IncomeSources { get; set; } = [];
    public ICollection<FinancialGoal> FinancialGoals { get; set; } = [];
}
