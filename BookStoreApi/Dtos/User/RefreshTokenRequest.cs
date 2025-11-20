using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Dtos.User;

public record RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string? RefreshToken { get; init; }
}