using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryManagementSystem.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IBookRepository _BooksRepo { get; }
        IAuthorRepository _AuthorsRepo { get; }
        ICategoryRepository _CategoriesRepo { get; }
        IBorrowRepository _BorrowsRepo { get; }
        IPaymentRepository _PaymentsRepo { get; }
        IRefreshTokenRepository _RefreshTokenRepo { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
