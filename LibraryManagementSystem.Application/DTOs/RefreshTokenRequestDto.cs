using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string? AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
