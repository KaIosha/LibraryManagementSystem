using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext dbContext) : base(dbContext) { }
        public async Task<List<Payment>> GetByBorrowIdAsync(Guid borrowId, CancellationToken ct = default)
        { 
            return await _dbContext.Payments.Where(p => p.BorrowId == borrowId).ToListAsync(ct);
        }

        public async Task<Payment?> GetByStripeSessionIdAsync(string sessionId, CancellationToken ct = default)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.StripeSessionId == sessionId, ct);
        }
    }
}
