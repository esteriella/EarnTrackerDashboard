using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.IntegrationDto;

public sealed record CreatePayStackTransactionDto
{
    /// <summary>The test customer's email address.</summary>
    /// <example>buyer@example.com</example>
    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; init; }

    /// <summary>The amount in the main currency unit, for example 1000 NGN.</summary>
    /// <example>1000.00</example>
    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; init; }

    /// <summary>The three-letter Paystack-supported currency code.</summary>
    /// <example>NGN</example>
    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = "NGN";

    /// <summary>A short description stored with the verified earning.</summary>
    /// <example>Paystack test payment</example>
    [Required, MinLength(1), MaxLength(120)]
    public string Description { get; init; } = "Paystack test payment";

    /// <summary>An optional absolute page Paystack returns to after checkout.</summary>
    /// <example>http://localhost:3000</example>
    [Url, MaxLength(500)]
    public string? CallbackUrl { get; init; }
}
