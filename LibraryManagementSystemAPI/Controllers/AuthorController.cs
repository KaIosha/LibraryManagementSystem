using LibraryManagementSystem.Application.DTOs.AuthorDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto dto)
        {
            var result = await _authorService.CreateAuthor(dto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAuthor([FromBody] UpdateAuthorDto dto)
        {
            var result = await _authorService.EditAuthor(dto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpDelete("{authorId:guid}")]
        public async Task<IActionResult> DeleteAuthor([FromRoute] Guid authorId)
        {
            var result = await _authorService.DeleteAuthor(authorId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAuthors([FromQuery] BaseQuery query)
        {
            var result = await _authorService.ViewAuthors(query);

            return Ok(result);
        }

        [HttpGet("{authorId:guid}")]
        public async Task<IActionResult> ViewAuthorDetails([FromRoute] Guid authorId)
        {
            var result = await _authorService.ViewAuthorDetails(authorId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [HttpGet("{authorId:guid}/books")]
        public async Task<IActionResult> ViewBooksByAuthor(Guid authorId, [FromQuery] BaseQuery query)
        {
            var result = await _authorService.ViewBooksByAuthor(authorId, query);

            return Ok(result);
        }
    }
}
