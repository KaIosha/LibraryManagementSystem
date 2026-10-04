using LibraryManagementSystem.Application.DTOs.BorrowDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            this._borrowService = borrowService;
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookDto dto)
        {
            var result = await _borrowService.BorrowBook(dto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPost("{borrowId:guid}/return")]
        public async Task<IActionResult> ReturnBook([FromRoute] Guid borrowId)
        {
            var result = await _borrowService.ReturnBook(borrowId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllBorrows([FromQuery] BaseQuery query)
        {
            var result = await _borrowService.ViewAllBorrows(query);

            return Ok(result);
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueBorrows([FromQuery] BaseQuery query)
        {
            var result = await _borrowService.GetOverdueBorrows(query);

            return Ok(result);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetBorrowsByMember([FromRoute] string memberId, [FromQuery] BaseQuery query)
        {
            try
            {
                var result = await _borrowService.GetBorrowsByMember(memberId, query);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{borrowId:guid}")]
        public async Task<IActionResult> GetBorrowById([FromRoute] Guid borrowId)
        {
            var result = await _borrowService.GetBorrowById(borrowId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
    }
}
