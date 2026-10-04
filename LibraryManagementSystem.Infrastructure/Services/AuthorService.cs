using LibraryManagementSystem.Application.DTOs.AuthorDtos;
using LibraryManagementSystem.Application.DTOs.CategoryDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthorService(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;
    }

    public async Task<AuthorResponseDto> CreateAuthor(CreateAuthorDto dto)
    {
        var nameExist = await _unitOfWork._AuthorsRepo.ExistsByNameAsync(dto.Name);

        if (nameExist)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "This name is already exist."
            };
        }

        var isExist = await _unitOfWork._AuthorsRepo.ExistsByEmailAsync(dto.Email);

        if (isExist)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "This email is already exist."
            };
        }

        var author = new Author
        {
            AuthorId = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            Nationality = dto.Nationality?.ToLower().Trim() ?? string.Empty,
            IsDeleted = false
        };

        await _unitOfWork._AuthorsRepo.AddAsync(author);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result == 0)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "The creation failed."
            };
        }

        return new AuthorResponseDto
        {
            AuthorId = author.AuthorId,
            Name = author.Name,
            Email = author.Email,
            Nationality = author.Nationality,
            BooksCount = 0,
            IsSuccess = true,
            Message = $"{author.Name} has been created."
        };
    }
    public async Task<AuthorResponseDto> EditAuthor(UpdateAuthorDto dto)
    {
        var author = await _unitOfWork._AuthorsRepo.GetByIdAsync(dto.AuthorId);

        if (author is null || author.IsDeleted)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "The author isn't exist."
            };
        }

        var email = dto.Email.Trim();
        var isExist = await _unitOfWork._AuthorsRepo.ExistsByEmailAsync(email);

        if (isExist && !author.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "This email already exists."
            };
        }

        var newName = (dto.Name ?? author.Name).Trim();
        if (!author.Name.Equals(newName, StringComparison.OrdinalIgnoreCase))
        {
            var nameTaken = await _unitOfWork._AuthorsRepo.ExistsByNameAsync(newName);
            if (nameTaken)
            {
                return new AuthorResponseDto
                {
                    IsSuccess = false,
                    Message = "This name already exists."
                };
            }
        }

        author.Name = newName;
        author.Email = email;
        author.Nationality = dto.Nationality?.ToLower().Trim() ?? author.Nationality;

        _unitOfWork._AuthorsRepo.Update(author);
        await _unitOfWork.SaveChangesAsync();

        return new AuthorResponseDto
        {
            AuthorId = author.AuthorId,
            Name = author.Name,
            Email = author.Email,
            Nationality = author.Nationality,
            BooksCount = 0,
            IsSuccess = true,
            Message = $"{author.Name} has been updated successfully."
        };
    }
    public async Task<AuthorResponseDto> DeleteAuthor(Guid authorId)
    {
        var author = await _unitOfWork._AuthorsRepo.GetByIdAsync(authorId);

        if (author is null)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "Author not found."
            };
        }

        if (author.IsDeleted)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "Author is already deleted."
            };
        }

        // Soft-delete: keep row + book history. Books stay linked;
        // BookService blocks new books for deleted authors.
        author.IsDeleted = true;

        _unitOfWork._AuthorsRepo.Update(author);
        await _unitOfWork.SaveChangesAsync();

        return new AuthorResponseDto
        {
            IsSuccess = true,
            Message = "Author deleted successfully (soft-deleted).",
            AuthorId = author.AuthorId,
            Name = author.Name
        };
    }
    public async Task<PagedResult<AuthorResponseDto>> ViewAuthors(BaseQuery query)
    {
        var dbQuery = _unitOfWork._AuthorsRepo.GetQueryable().Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            dbQuery = dbQuery.Where(a => a.Name.Contains(term));
        }

        var total = await dbQuery.CountAsync();

        var page = await dbQuery
            .OrderBy(a => a.Name)
            .Skip(query.Skip)
            .Take(query.Take)
            .ToListAsync();

        var counts = await _unitOfWork._BooksRepo.CountGroupedByAuthorAsync();

        var items = new List<AuthorResponseDto>();

        foreach (var author in page)
        {
            counts.TryGetValue(author.AuthorId, out var bookCount);

            items.Add(new AuthorResponseDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Email = author.Email,
                Nationality = author.Nationality,
                BooksCount = bookCount,
                IsSuccess = true,
                Message = "Authors retrieved successfully."
            });
        }

        return new PagedResult<AuthorResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
    public async Task<AuthorResponseDto> ViewAuthorDetails(Guid authorId)
    {
        var author = await _unitOfWork._AuthorsRepo.GetByIdAsync(authorId);

        if (author is null || author.IsDeleted)
        {
            return new AuthorResponseDto
            {
                IsSuccess = false,
                Message = "Author not found."
            };
        }

        var counts = await _unitOfWork._BooksRepo.CountGroupedByAuthorAsync();
        counts.TryGetValue(author.AuthorId, out var bookCount);

        return new AuthorResponseDto
        {
            AuthorId = author.AuthorId,
            Name = author.Name,
            Email = author.Email,
            Nationality = author.Nationality,
            BooksCount = bookCount,
            IsSuccess = true,
            Message = "Author retrieved successfully."
        };
    }
    public async Task<PagedResult<BookDataDto>> ViewBooksByAuthor(Guid authorId, BaseQuery query)
    {
        var author = await _unitOfWork._AuthorsRepo.GetByIdAsync(authorId);

        if (author is null || author.IsDeleted)
        {
            return new PagedResult<BookDataDto>
            {
                Items = new(),
                TotalCount = 0,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        var booksQuery = _unitOfWork._BooksRepo.GetQueryable().Where(b => b.AuthorId == authorId && !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();

            booksQuery = booksQuery.Where(b =>
                b.Title.Contains(term) ||
                b.ISBN.Contains(term));
        }

        var total = await booksQuery.CountAsync();

        var pageItems = await booksQuery
            .OrderBy(b => b.Title)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BookDataDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                Availability = b.Availability
            })
            .ToListAsync();

        return new PagedResult<BookDataDto>
        {
            Items = pageItems,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
