using LibraryManagementSystem.Application.DTOs.AuthDtos;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            var result = await _authService.ConfirmEmailAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ResendConfirmationAsync(dto.Email);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                result.Token,
                result.RefreshToken,
                result.ExpireAt
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                result.Token,
                result.RefreshToken,
                result.ExpireAt
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.LogOutAsync(dto.RefreshToken);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message
            });
        }

        [HttpPost("add-staff")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> AddStaff([FromBody] CreateUserDto dto)
        {
            var result = await _authService.AddStaffByAdminAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { result.IsSuccess, result.Message });
            }

            return Ok(new
            {
                result.IsSuccess,
                result.Message,
                result.UserName,
                result.Email
            });
        }
    }
}
