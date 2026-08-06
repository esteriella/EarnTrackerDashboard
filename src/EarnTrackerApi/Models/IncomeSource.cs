namespace EarnTrackerApi.Models;

public sealed class IncomeSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string Provider { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public ICollection<EarningTransaction> Transactions { get; set; } = [];
}
