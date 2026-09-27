using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Application.DTOs.AuthDtos;

namespace LibraryManagementSystem.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthResponseDto> ResendConfirmationAsync(string email);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<AuthResponseDto> LogOutAsync(string refreshToken);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);


        // As Admin
        Task<AuthResponseDto> AddStaffByAdminAsync(CreateUserDto dto); // dto.Role = Staff | Librarian
        
       
    }
}
