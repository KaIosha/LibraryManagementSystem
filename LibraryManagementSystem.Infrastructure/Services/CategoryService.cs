using LibraryManagementSystem.Application.DTOs.CategoryDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponseDto> CreateCategory(CreateCategoryDto dto)
    {
        var isExsit = await _unitOfWork._CategoriesRepo.ExistsByNameAsync(dto.Name);
        if (isExsit)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "This Name is already exsit"
            };
        }

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim()
        };

        await _unitOfWork._CategoriesRepo.AddAsync(category);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result == 0)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "The creation failed"
            };
        }

        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            BooksCount = 0,
            IsSuccess = true,
            Message = $"{category.Name} has been created."
        };

    }
    public async Task<CategoryResponseDto> UpdateCategory(UpdateCategoryDto dto)
    {
        var category = await _unitOfWork._CategoriesRepo.GetByIdAsync(dto.CategoryId);
        if (category is null)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "The category isn't exsit"
            };
        }

        var name = dto.Name.Trim();

        var isExist = await _unitOfWork._CategoriesRepo.ExistsByNameAsync(name);

        if (isExist && !category.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "This category name already exists."
            };
        }


        category.Name = name;
        category.Description = dto.Description?.Trim();

        _unitOfWork._CategoriesRepo.Update(category);

        var result = await _unitOfWork.SaveChangesAsync();

        if (result == 0)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "Category update failed."
            };
        }

        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description,
            BooksCount = 0,
            IsSuccess = true,
            Message = $"{category.Name} has been updated successfully."
        };

    }
    public async Task<CategoryResponseDto> DeleteCategory(Guid categoryId)
    {
        var category = await _unitOfWork._CategoriesRepo.GetByIdAsync(categoryId);

        if (category is null)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "Category not found."
            };
        }

        var booksExist = await _unitOfWork._BooksRepo.ExistsByCategoryAsync(categoryId);

        if (booksExist)
        {
            return new CategoryResponseDto
            {
                IsSuccess = false,
                Message = "Cannot delete category, it has books."
            };
        }

        _unitOfWork._CategoriesRepo.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return new CategoryResponseDto
        {
            IsSuccess = true,
            Message = "Category deleted successfully.",
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }
    public async Task<PagedResult<BookDataDto>> ViewBooksInACategory(Guid categoryId, BaseQuery query)
    {
        var category = await _unitOfWork._CategoriesRepo.GetByIdAsync(categoryId);

        if (category is null)
        {
            return new PagedResult<BookDataDto>
            {
                Items = new(),
                TotalCount = 0,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        var (items, total) = await _unitOfWork._BooksRepo.GetPagedByCategoryAsync(categoryId, query.SearchTerm, query.Skip, query.Take);

        return new PagedResult<BookDataDto>
        {
            Items = items.Select(b => new BookDataDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                Availability = b.Availability
            }).ToList(),
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
    public async Task<PagedResult<CategoryResponseDto>> ViewCategories(BaseQuery query)
    {
        var allCategories = await _unitOfWork._CategoriesRepo.GetAllAsync();
        var counts = await _unitOfWork._BooksRepo.CountGroupedByCategoryAsync();

        var filtered = new List<CategoryResponseDto>();

        foreach (var category in allCategories)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();

                if (!category.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            counts.TryGetValue(category.CategoryId, out var bookCount);

            filtered.Add(new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                BooksCount = bookCount,
                IsSuccess = true,
                Message = "Categories retrieved successfully."
            });
        }

        var total = filtered.Count;
        var pageItems = filtered.OrderBy(x => x.Name).Skip(query.Skip).Take(query.Take).ToList();

        return new PagedResult<CategoryResponseDto>
        {
            Items = pageItems,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
