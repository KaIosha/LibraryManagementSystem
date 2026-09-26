using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Application.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T :class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await _dbSet.FindAsync(new object[] { id }, ct);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) => await _dbSet.ToListAsync(ct);

        public async Task AddAsync(T entity, CancellationToken ct = default) => await _dbSet.AddAsync(entity, ct);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}
