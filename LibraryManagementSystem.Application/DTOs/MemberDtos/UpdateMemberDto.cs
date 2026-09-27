using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.MemberDtos
{
    public class UpdateMemberDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
