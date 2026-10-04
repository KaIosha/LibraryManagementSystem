using LibraryManagementSystem.Application.Interfaces.IRepositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class BookRepository : GenericRepository<Book>,IBookRepository
    {
        public BookRepository(ApplicationDbContext dbContext) : base(dbContext) { }
        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author, CancellationToken ct = default)
        {
            return await _dbContext.Books
                .Where(b => !b.IsDeleted && b.Author.Name == author)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByCategoryAsync(Guid categoryId, CancellationToken ct = default)
        {
            return await _dbContext.Books.AnyAsync(b => !b.IsDeleted && b.CategoryId == categoryId, ct);
        }

        public async Task<Dictionary<Guid, int>> CountGroupedByCategoryAsync(CancellationToken ct = default)
        {
            return await _dbContext.Books
                .Where(b => !b.IsDeleted)
                .GroupBy(b => b.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);
        }

        public async Task<bool> ExistsByAuthorAsync(Guid authorId, CancellationToken ct = default)
        {
            return await _dbContext.Books.AnyAsync(b => !b.IsDeleted && b.AuthorId == authorId, ct);
        }

        public async Task<Dictionary<Guid, int>> CountGroupedByAuthorAsync(CancellationToken ct = default)
        {
            return await _dbContext.Books
                .Where(b => !b.IsDeleted)
                .GroupBy(b => b.AuthorId)
                .Select(g => new { AuthorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AuthorId, x => x.Count, ct);
        }

        public async Task<(IEnumerable<Book> Items, int TotalCount)> GetPagedByCategoryAsync(Guid categoryId, string? searchTerm, int skip, int take, CancellationToken ct = default)
        {
            var query = _dbContext.Books.Where(b => !b.IsDeleted && b.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(b => b.Title.Contains(term) || b.ISBN.Contains(term));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<Book?> IsAvailable(Guid bookId)
        {
            return await _dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b =>
                    b.BookId == bookId &&
                    !b.IsDeleted &&
                    b.Availability);
        }
        public IQueryable<Book> GetQueryable()
        {
            return _dbContext.Books.AsNoTracking();
        }
    }
}
