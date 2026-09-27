using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.AuthDtos
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
