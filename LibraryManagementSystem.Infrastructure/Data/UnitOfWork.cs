using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagementSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private IDbContextTransaction? _transaction;
        public IBookRepository _BooksRepo { get;}
        public IAuthorRepository _AuthorsRepo{ get;}
        public ICategoryRepository _CategoriesRepo { get;}
        public IBorrowRepository _BorrowsRepo { get;}
        public IPaymentRepository _PaymentsRepo { get;}
        public IRefreshTokenRepository _RefreshTokenRepo { get; }
        public UnitOfWork(ApplicationDbContext dbContext , 
            IBookRepository Books,
            IAuthorRepository Authors,
            ICategoryRepository Categories,
            IBorrowRepository Borrows,
            IPaymentRepository Payments,
            IRefreshTokenRepository RefreshToken)
        {
            _dbContext = dbContext;
            _BooksRepo = Books;
            _AuthorsRepo = Authors;
            _CategoriesRepo = Categories;
            _BorrowsRepo = Borrows;
            _PaymentsRepo = Payments;
            _RefreshTokenRepo = RefreshToken;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task BeginTransactionAsync(CancellationToken ct)
        {
            if (_transaction != null) return;
            _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct)
        {

            if (_transaction is null) return;

            try
            {
                await _transaction.CommitAsync(ct);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct)
        {
            if (_transaction is null) return;

            try
            {
                await _transaction.RollbackAsync(ct);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
