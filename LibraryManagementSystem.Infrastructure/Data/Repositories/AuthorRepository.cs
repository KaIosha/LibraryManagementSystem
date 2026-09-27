using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbContext.Authors.AnyAsync(a => a.Email == email);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            var normalized = name.Trim().ToLower();
            return await _dbContext.Authors.AnyAsync(a => a.Name.ToLower() == normalized);
        }

        public IQueryable<Author> GetQueryable()
        {
            return _dbContext.Authors.AsNoTracking();
        }
    }
}
