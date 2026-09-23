using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Domain.Entities
{
    public class Borrow
    {
        public Guid BorrowId { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;
        public string MemberId { get; set; } = string.Empty;
        public ApplicationUser Member { get; set; } = null!;

        public string StaffId { get; set; } = string.Empty;
        public ApplicationUser Staff { get; set; } = null!;
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public BorrowStatus Status { get; set; } = BorrowStatus.Borrowed;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // first one failed second success  or  pay today 2$ next week 5$
    }
}
