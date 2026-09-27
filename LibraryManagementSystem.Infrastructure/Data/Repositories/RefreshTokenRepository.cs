using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {


        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext) { }


        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);
        }
    }
}
