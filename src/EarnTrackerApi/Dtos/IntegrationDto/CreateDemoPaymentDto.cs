using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.IntegrationDto;

public sealed record CreateDemoPaymentDto
{
    /// <summary>The fictional amount added to the signed-in user's dashboard.</summary>
    /// <example>250.00</example>
    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; init; }

    /// <summary>The three-letter currency used for this demo entry.</summary>
    /// <example>USD</example>
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = "USD";

    /// <summary>A short label that identifies the fictional payment.</summary>
    /// <example>Portfolio website project</example>
    [Required, MinLength(1), MaxLength(120)]
    public string Description { get; init; } = "Sample freelance project";
}
