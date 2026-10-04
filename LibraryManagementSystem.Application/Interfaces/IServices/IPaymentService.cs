
using LibraryManagementSystem.Application.DTOs.PaymentDtos;

namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreateCheckoutSessionAsync(CreateCheckoutSessionDto dto);
    Task<PaymentResponseDto> RecordCashPaymentAsync(CreateCashPaymentDto dto);
    Task<List<PaymentResponseDto>> GetPaymentsByBorrowIdAsync(Guid borrowId);
    Task<PaymentResponseDto> VerifyCheckoutSessionAsync(string sessionId);
}
