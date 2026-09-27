namespace LibraryManagementSystem.Application.DTOs.MemberDtos
{
    public class MemberResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public int ActiveBorrowsCount { get; set; }

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
