using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAuthService _emailService;
        public TestController(IAuthService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(RegisterDto dto)
        {
            await _emailService.RegisterAsync(dto);
            return Ok();
        }
    }
}
