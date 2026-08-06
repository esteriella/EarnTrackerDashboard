namespace EarnTrackerApi.Models;

public sealed class FinancialGoal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public decimal TargetAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateOnly StartDate { get; set; }
    public DateOnly TargetDate { get; set; }

    public User? User { get; set; }
}
