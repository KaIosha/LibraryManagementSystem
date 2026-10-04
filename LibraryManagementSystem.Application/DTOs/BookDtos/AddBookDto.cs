using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.BookDtos
{
    public class AddBookDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Genre { get; set; }

        [StringLength(50)]
        public string? Language { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
}
