using System;
using System.Collections.Generic;
using System.Text;
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
                .Where(b => b.Author.Name == author)
                .ToListAsync(ct);
        }
    }
}
