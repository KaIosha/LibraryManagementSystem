using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<List<Payment>> GetByBorrowIdAsync(Guid borrowId, CancellationToken ct = default);
    Task<Payment?> GetByStripeSessionIdAsync(string sessionId, CancellationToken ct = default);
}
