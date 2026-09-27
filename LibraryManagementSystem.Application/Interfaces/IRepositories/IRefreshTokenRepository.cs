using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces.IRepositories
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken> 
    {
        Task<RefreshToken?> GetByTokenAsync(string token);

        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    }
}
