using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
