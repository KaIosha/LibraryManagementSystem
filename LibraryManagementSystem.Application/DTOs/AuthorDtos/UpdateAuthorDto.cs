using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.AuthorDtos
{
    public class UpdateAuthorDto
    {
        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Nationality { get; set; }
    }
}
