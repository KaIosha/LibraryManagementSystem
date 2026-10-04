using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class BorrowRepository : GenericRepository<Borrow>, IBorrowRepository
    {
        public BorrowRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<Borrow>> GetByMemberIdAsync(string memberId, CancellationToken ct = default)
        {
            return await _dbContext.Borrows
                .Where(b => b.MemberId == memberId)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsActiveBorrowByBookAsync(Guid bookId, CancellationToken ct = default)
        {
            return await _dbContext.Borrows.AnyAsync(b =>
                b.BookId == bookId &&
                (b.Status == BorrowStatus.Borrowed || b.Status == BorrowStatus.Overdue), ct);
        }

        public async Task<int> CountActiveByMemberAsync(string memberId, CancellationToken ct = default)
        {
            return await _dbContext.Borrows.CountAsync(b =>
                b.MemberId == memberId &&
                (b.Status == BorrowStatus.Borrowed || b.Status == BorrowStatus.Overdue), ct);
        }

        public IQueryable<Borrow> GetQueryable()
        {
            return _dbContext.Borrows.AsNoTracking();
        }
    }
}
