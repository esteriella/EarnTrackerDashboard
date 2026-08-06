using System.ComponentModel.DataAnnotations;

namespace EarnTrackerApi.Dtos.AuthDto;

public sealed record RefreshTokenDto
{
    [Required]
    public required string RefreshToken { get; set; }
}
