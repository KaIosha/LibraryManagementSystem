using LibraryManagementSystem.Application.DTOs.AuthDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.DTOs.MemberDtos;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMemberService _memberService;

        public MemberController(IAuthService authService, IMemberService memberService)
        {
            _authService = authService;
            _memberService = memberService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

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

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPut("{memberId}")]
        public async Task<IActionResult> UpdateMember([FromRoute] string memberId, [FromBody] UpdateMemberDto dto)
        {
            var result = await _memberService.UpdateMemberAsync(memberId, dto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian")]
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> DeleteMember([FromRoute] string memberId)
        {
            var result = await _memberService.DeleteMemberAsync(memberId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian")]
        [HttpPost("{memberId}/reactivate")]
        public async Task<IActionResult> ReactivateMember([FromRoute] string memberId)
        {
            var result = await _memberService.ReactivateMemberAsync(memberId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

       //[Authorize]
        [HttpGet("{memberId}")]
        public async Task<IActionResult> ViewMemberProfile([FromRoute] string memberId)
        {
            var result = await _memberService.GetMemberProfileAsync(memberId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        //[Authorize]
        [HttpGet("{memberId}/borrows")]
        public async Task<IActionResult> ListMemberBorrows([FromRoute] string memberId, [FromQuery] BaseQuery query)
        {
            var result = await _memberService.GetMemberBorrowsAsync(memberId, query);

            return Ok(result);
        }
    }
}
