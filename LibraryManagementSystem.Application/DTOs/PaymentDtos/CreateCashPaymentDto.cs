using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs.PaymentDtos;

public class CreateCashPaymentDto
{
    [Required]
    public Guid BorrowId { get; set; }

    [Required]
    [Range(0.5, 10000, ErrorMessage = "Amount must be between 0.5 and 10000.")]
    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}


