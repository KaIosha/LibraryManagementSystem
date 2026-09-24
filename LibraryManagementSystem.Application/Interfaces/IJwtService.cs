using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Interfaces
{
    public interface IJwtService
    {
        Task<JwtResponseDto> CreateJwtTokenAsync(ApplicationUser user);
    }
}
