using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.CategoryDtos
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
