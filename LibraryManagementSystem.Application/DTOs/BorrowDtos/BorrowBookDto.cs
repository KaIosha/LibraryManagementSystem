using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.BorrowDtos
{
    public class BorrowBookDto
    {
        [Required]
        public Guid BookId { get; set; }

        [Required]
        public string MemberId { get; set; } = string.Empty;

        [Required]
        public string StaffId { get; set; } = string.Empty;
    }
}
