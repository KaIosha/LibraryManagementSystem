using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class BorrowRepository : GenericRepository<Borrow>, IBorrowRepository
    {
        public BorrowRepository(ApplicationDbContext dbContext) : base(dbContext) { }
    }
}
