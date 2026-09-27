using LibraryManagementSystem.Application.DTOs.CategoryDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Roles = "Librarian,Staff")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategory(dto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Librarian,Staff")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategory(dto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize(Roles = "Librarian,Staff")]
        [HttpDelete("{categoryId:guid}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId)
        {
            var result = await _categoryService.DeleteCategory(categoryId);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCategories([FromQuery] BaseQuery query)
        {
            var result = await _categoryService.ViewCategories(query);
            return Ok(result);
        }

        [HttpGet("{categoryId:guid}/books")]
        public async Task<IActionResult> ViewBooksInACategory(Guid categoryId, [FromQuery] BaseQuery query)
        {
            var result = await _categoryService.ViewBooksInACategory(categoryId, query);
            return Ok(result);
        }
    }
}
