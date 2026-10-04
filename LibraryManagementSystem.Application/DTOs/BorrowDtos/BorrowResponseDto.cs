namespace LibraryManagementSystem.Application.DTOs.BorrowDtos
{
    public class BorrowResponseDto
    {
        public Guid BorrowId { get; set; }

        public Guid BookId { get; set; }

        public string BookTitle { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public string MemberId { get; set; } = string.Empty;

        public string MemberName { get; set; } = string.Empty;

        public string StaffId { get; set; } = string.Empty;

        public string StaffName { get; set; } = string.Empty;

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
