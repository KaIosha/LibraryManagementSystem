using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
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
    }
}
