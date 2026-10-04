using LibraryManagementSystem.Application.DTOs.BookDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            this._bookService = bookService;
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPost("create")]
        public async Task<IActionResult> AddNewBook([FromBody] AddBookDto dto)
        {
            var result = await _bookService.AddNewBook(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookDto dto)
        {
            var result = await _bookService.UpdateBook(dto);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        //[Authorize(Roles = "Librarian,Staff")]
        [HttpDelete("{bookId:guid}")]
        public async Task<IActionResult> DeleteBook([FromRoute] Guid bookId)
        {
            var result = await _bookService.DeleteBook(bookId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllBooks([FromQuery] BaseQuery query)
        {
            var result = await _bookService.ViewAllBooks(query);

            return Ok(result);
        }

        [HttpGet("{bookId:guid}")]
        public async Task<IActionResult> GetBookById([FromRoute] Guid bookId)
        {
            var result = await _bookService.GetBookById(bookId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> GetBooksByCategory([FromRoute] Guid categoryId, [FromQuery] BaseQuery query)
        {
            var result = await _bookService.GetBooksByCategory(categoryId, query);

            return Ok(result);
        }

        [HttpGet("author/{authorId:guid}")]
        public async Task<IActionResult> GetBooksByAuthor([FromRoute] Guid authorId, [FromQuery] BaseQuery query)
        {
            var result = await _bookService.GetBooksByAuthor(authorId, query);

            return Ok(result);
        }

        [HttpGet("{bookId:guid}/availability")]
        public async Task<IActionResult> IsBookAvailable([FromRoute] Guid bookId)
        {
            var result = await _bookService.IsBookAvailable(bookId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetBook([FromQuery] string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return BadRequest("Search term is required.");
            }

            var result = await _bookService.GetBook(search.Trim());

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
    }
}
