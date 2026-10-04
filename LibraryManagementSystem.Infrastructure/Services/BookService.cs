using LibraryManagementSystem.Application.DTOs.BookDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;
    }

    public async Task<BookResponseDto> AddNewBook(AddBookDto dto)
    {
        var isbn = dto.ISBN.Trim();

        // Check ALL rows including soft-deleted (DB unique index covers them too).
        var existingBook = await _unitOfWork._BooksRepo
            .GetQueryable()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.ISBN == isbn);

        if (existingBook is not null)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "A book with this ISBN already exists"
            };
        }

        var author = await _unitOfWork._AuthorsRepo
            .GetByIdAsync(dto.AuthorId);

        if (author is null || author.IsDeleted)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Author doesn't exist"
            };
        }

        var category = await _unitOfWork._CategoriesRepo
            .GetByIdAsync(dto.CategoryId);

        if (category is null || category.IsDeleted)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Category doesn't exist"
            };
        }

        var book = new Book
        {
            BookId = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            ISBN = isbn,
            Genre = dto.Genre?.Trim() ?? string.Empty,
            Language = dto.Language?.Trim() ?? string.Empty,
            AuthorId = dto.AuthorId,
            CategoryId = dto.CategoryId,
            Availability = true,
            IsDeleted = false
        };

        await _unitOfWork._BooksRepo.AddAsync(book);

        await _unitOfWork.SaveChangesAsync();

        return new BookResponseDto
        {
            IsSuccess = true,
            BookId = book.BookId,
            ISBN = book.ISBN,
            Title = book.Title,
            AuthorId = book.AuthorId,
            AuthorName = author.Name,
            CategoryId = book.CategoryId,
            CategoryName = category.Name,
            Language = book.Language,
            Availability = book.Availability,
            Genre = book.Genre,
            Message = "Book added successfully"
        };
    }

    public async Task<BookResponseDto> DeleteBook(Guid bookId)
    {
        // GetByIdAsync (FindAsync) bypasses the soft-delete query filter,
        // so we can distinguish "not found" from "already deleted".
        var book = await _unitOfWork._BooksRepo.GetByIdAsync(bookId);
        if (book is null)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Book not found."
            };
        }

        if (book.IsDeleted)
        {
            return new BookResponseDto
            {
                BookId = book.BookId,
                Title = book.Title,
                ISBN = book.ISBN,
                Availability = false,
                IsSuccess = false,
                Message = "Book is already deleted."
            };
        }

        // A currently-borrowed book must be returned first.
        // Availability == false alone is NOT enough (it also means borrowed),
        // so check the active borrow record explicitly.
        var isBorrowed = await _unitOfWork._BorrowsRepo.ExistsActiveBorrowByBookAsync(bookId);
        if (isBorrowed)
        {
            return new BookResponseDto
            {
                BookId = book.BookId,
                Title = book.Title,
                ISBN = book.ISBN,
                Availability = book.Availability,
                IsSuccess = false,
                Message = "Cannot delete book, it is currently borrowed."
            };
        }

        // Soft-delete: keep the row, hide it via query filter.
        book.IsDeleted = true;
        book.Availability = false;

        _unitOfWork._BooksRepo.Update(book);
        await _unitOfWork.SaveChangesAsync();

        return new BookResponseDto
        {
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            Genre = book.Genre,
            Language = book.Language,
            Availability = false,
            AuthorId = book.AuthorId,
            CategoryId = book.CategoryId,
            IsSuccess = true,
            Message = "Book deleted successfully (soft-deleted)."
        };
    }

    public async Task<PagedResult<BookResponseDto>> GetBooksByCategory(Guid categoryId, BaseQuery query)
    {
        var category = await _unitOfWork._CategoriesRepo.GetByIdAsync(categoryId);
        if (category is null || category.IsDeleted)
        {
            return new PagedResult<BookResponseDto>
            {
                Items = new(),
                TotalCount = 0,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        var booksQuery = _unitOfWork._BooksRepo.GetQueryable()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.CategoryId == categoryId && !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            booksQuery = booksQuery.Where(b =>
                b.Title.Contains(term) ||
                b.ISBN.Contains(term));
        }

        var total = await booksQuery.CountAsync();

        var items = await booksQuery
            .OrderBy(b => b.Title)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                Genre = b.Genre,
                Language = b.Language,
                Availability = b.Availability,
                AuthorId = b.AuthorId,
                AuthorName = b.Author.Name,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                IsSuccess = true,
                Message = "Book retrieved successfully."
            })
            .ToListAsync();

        return new PagedResult<BookResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<PagedResult<BookResponseDto>> GetBooksByAuthor(Guid authorId, BaseQuery query)
    {
        var author = await _unitOfWork._AuthorsRepo.GetByIdAsync(authorId);
        if (author is null || author.IsDeleted)
        {
            return new PagedResult<BookResponseDto>
            {
                Items = new(),
                TotalCount = 0,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        var booksQuery = _unitOfWork._BooksRepo.GetQueryable()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.AuthorId == authorId && !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            booksQuery = booksQuery.Where(b =>
                b.Title.Contains(term) ||
                b.ISBN.Contains(term));
        }

        var total = await booksQuery.CountAsync();

        var items = await booksQuery
            .OrderBy(b => b.Title)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                Genre = b.Genre,
                Language = b.Language,
                Availability = b.Availability,
                AuthorId = b.AuthorId,
                AuthorName = b.Author.Name,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                IsSuccess = true,
                Message = "Book retrieved successfully."
            })
            .ToListAsync();

        return new PagedResult<BookResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<BookResponseDto> GetBookById(Guid bookId)
    {
        // Single query; global query filter hides soft-deleted books.
        var book = await _unitOfWork._BooksRepo.GetQueryable()
                      .Include(b => b.Author)
                      .Include(b => b.Category)
                      .FirstOrDefaultAsync(b => b.BookId == bookId);

        if (book is null)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Book doesn't exist"
            };
        }

        return new BookResponseDto
        {
            IsSuccess = true,
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            Language = book.Language,
            Availability = book.Availability,
            Genre = book.Genre,
            Message = "Book exists"
        };
    }

    public async Task<BookResponseDto> GetBook(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Search term is required."
            };
        }

        var term = search.Trim();

        // Partial match instead of exact == ; single query with includes.
        var book = await _unitOfWork._BooksRepo
            .GetQueryable()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.Title.Contains(term) || b.ISBN.Contains(term))
            .OrderBy(b => b.Title)
            .FirstOrDefaultAsync();

        if (book is null)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Book doesn't exist"
            };
        }

        return new BookResponseDto
        {
            IsSuccess = true,
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            Language = book.Language,
            Availability = book.Availability,
            Genre = book.Genre,
            Message = "Book exists"
        };
    }

    public async Task<BookResponseDto> IsBookAvailable(Guid bookId)
    {
        var book = await _unitOfWork._BooksRepo.IsAvailable(bookId);
        if (book is null)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Book isn't available"
            };
        }

        return new BookResponseDto
        {
            IsSuccess = true,
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            Language = book.Language,
            Availability = book.Availability,
            Genre = book.Genre,
            Message = "Book is available"
        };
    }

    public async Task<BookResponseDto> UpdateBook(UpdateBookDto dto)
    {
        
        var book = await _unitOfWork._BooksRepo.GetByIdAsync(dto.BookId);
        if (book is null || book.IsDeleted)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Book doesn't exist"
            };
        }

        var category = await _unitOfWork._CategoriesRepo.GetByIdAsync(dto.CategoryId);
        if (category is null || category.IsDeleted)
        {
            return new BookResponseDto
            {
                IsSuccess = false,
                Message = "Category doesn't exist"
            };
        }

        var newIsbn = dto.ISBN.Trim();
        if (!book.ISBN.Equals(newIsbn, StringComparison.Ordinal))
        {
            var isbnTaken = await _unitOfWork._BooksRepo
                .GetQueryable()
                .IgnoreQueryFilters()
                .AnyAsync(b => b.ISBN == newIsbn && b.BookId != book.BookId);

            if (isbnTaken)
            {
                return new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "A book with this ISBN already exists"
                };
            }

            book.ISBN = newIsbn;
        }

        book.Title = dto.Title.Trim();
        book.Genre = dto.Genre?.Trim() ?? string.Empty;
        book.Language = dto.Language?.Trim() ?? string.Empty;
        book.CategoryId = dto.CategoryId;
      

        _unitOfWork._BooksRepo.Update(book);
        await _unitOfWork.SaveChangesAsync();

        return new BookResponseDto
        {
            IsSuccess = true,
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            Genre = book.Genre,
            Language = book.Language,
            Availability = book.Availability,
            AuthorId = book.AuthorId,
            CategoryId = book.CategoryId,
            CategoryName = category.Name,
            Message = "Book updated successfully"
        };
    }

    public async Task<PagedResult<BookResponseDto>> ViewAllBooks(BaseQuery query)
    {
        IQueryable<Book> booksQuery = _unitOfWork._BooksRepo.GetQueryable()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            booksQuery = booksQuery.Where(b =>
                b.Title.Contains(term) ||
                b.ISBN.Contains(term));
        }

        var total = await booksQuery.CountAsync();

        var items = await booksQuery
            .OrderBy(b => b.Title)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                Genre = b.Genre,
                Language = b.Language,
                Availability = b.Availability,
                AuthorId = b.AuthorId,
                AuthorName = b.Author.Name,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                IsSuccess = true,
                Message = "Book retrieved successfully."
            })
            .ToListAsync();

        return new PagedResult<BookResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
