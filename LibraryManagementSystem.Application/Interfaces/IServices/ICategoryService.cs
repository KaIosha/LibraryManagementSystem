using LibraryManagementSystem.Application.DTOs.CategoryDtos;
using LibraryManagementSystem.Application.DTOs.Common;

namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface ICategoryService
{
    //• Create categories 
    Task<CategoryResponseDto> CreateCategory(CreateCategoryDto dto);
    //• Update categories 
    Task<CategoryResponseDto> UpdateCategory(UpdateCategoryDto dto);
    //• Delete categories 
    Task<CategoryResponseDto> DeleteCategory(Guid categoryId);
    //• View categories 
    Task<PagedResult<CategoryResponseDto>> ViewCategories(BaseQuery query);
    //• Retrieve books belonging to a category
    Task<PagedResult<BookDataDto>> ViewBooksInACategory(Guid categoryId, BaseQuery query);

}
