namespace EarnTrackerApi.Models;

public sealed class EarningTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IncomeSourceId { get; set; }
    public required string ExternalId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Completed";
    public string? Description { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public IncomeSource? IncomeSource { get; set; }
}
