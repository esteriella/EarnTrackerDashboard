using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.IntegrationDto;

public sealed record CreatePayPalOrderDto
{
    /// <summary>The amount the sandbox buyer will approve.</summary>
    /// <example>10.00</example>
    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; init; }

    /// <summary>The three-letter PayPal-supported currency code.</summary>
    /// <example>USD</example>
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = "USD";

    /// <summary>A short description shown with the purchase.</summary>
    /// <example>Freelancer earnings tracker sandbox test</example>
    [Required, MinLength(1), MaxLength(127)]
    public string Description { get; init; } =
        "Freelancer earnings tracker sandbox test";
}
