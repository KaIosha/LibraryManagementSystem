using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken> 
    {
    }
}
