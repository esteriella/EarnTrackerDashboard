using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.AuthDto;

public sealed record RegisterDto
{
    /// <summary>The freelancer's display name.</summary>
    /// <example>Ada Lovelace</example>
    [Required, MinLength(2), MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>The unique email used to sign in.</summary>
    /// <example>ada@example.com</example>
    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; set; }

    /// <summary>An 8–12 character password with upper and lowercase letters, a number, and a special character.</summary>
    /// <example>Earn#2026</example>
    [Required, DataType(DataType.Password), MinLength(8), MaxLength(12)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,12}$",
        ErrorMessage = "Password must contain uppercase and lowercase letters, a number, and a special character.")]
    public required string Password { get; set; }
}
