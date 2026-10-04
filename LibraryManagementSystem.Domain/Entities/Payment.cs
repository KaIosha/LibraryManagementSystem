using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Domain.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }

        public Guid BorrowId { get; set; }
        public Borrow Borrow { get; set; } = null!;

        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Notes { get; set; } = string.Empty;
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
    }
}
