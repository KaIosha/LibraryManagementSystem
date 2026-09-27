namespace LibraryManagementSystem.Application.DTOs.MemberDtos
{
    public class MemberBorrowedBookDto
    {
        public Guid BorrowId { get; set; }

        public Guid BookId { get; set; }

        public string BookTitle { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
