using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.CategoryDtos
{
    public class UpdateCategoryDto
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
