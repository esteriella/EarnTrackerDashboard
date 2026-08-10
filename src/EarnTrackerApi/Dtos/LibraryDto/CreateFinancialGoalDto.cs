using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.LibraryDto;

public sealed record CreateFinancialGoalDto : IValidatableObject
{
    /// <summary>A short name that explains the purpose of the goal.</summary>
    /// <example>September income goal</example>
    [Required, MinLength(2), MaxLength(100)]
    public required string Name { get; init; }

    /// <summary>The net earnings amount required to achieve the goal.</summary>
    /// <example>5000.00</example>
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal TargetAmount { get; init; }

    /// <summary>The three-letter currency used to measure progress.</summary>
    /// <example>USD</example>
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = "USD";

    /// <summary>The first date whose earnings count toward the goal.</summary>
    /// <example>2026-09-01</example>
    public DateOnly StartDate { get; init; }

    /// <summary>The final date whose earnings count toward the goal.</summary>
    /// <example>2026-09-30</example>
    public DateOnly TargetDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate == default)
        {
            yield return new ValidationResult(
                "Start date is required.",
                [nameof(StartDate)]);
        }

        if (TargetDate == default)
        {
            yield return new ValidationResult(
                "Target date is required.",
                [nameof(TargetDate)]);
        }
        else if (StartDate != default && TargetDate < StartDate)
        {
            yield return new ValidationResult(
                "Target date must be on or after the start date.",
                [nameof(TargetDate)]);
        }
    }
}
