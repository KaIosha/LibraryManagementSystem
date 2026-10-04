
using LibraryManagementSystem.Domain.Enums;

namespace LibraryManagementSystem.Application.DTOs.PaymentDtos;

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }

    public Guid BorrowId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; } 

    public DateTime PaymentDate { get; set; }

    public string? StripeSessionId { get; set; }

    public string? SessionUrl { get; set; }

    public bool IsSuccess { get; set; } = false;

    public string Message { get; set; } = string.Empty;
}


