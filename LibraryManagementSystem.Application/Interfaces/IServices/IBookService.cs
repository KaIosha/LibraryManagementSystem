
using LibraryManagementSystem.Application.DTOs.BookDtos;
using LibraryManagementSystem.Application.DTOs.Common;


namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface IBookService
{
    //• Add a new book 
    Task<BookResponseDto> AddNewBook(AddBookDto dto);
    //• Update book information 
    Task<BookResponseDto> UpdateBook(UpdateBookDto dto);
    //• Delete a book (soft-delete: IsDeleted = true, Availability = false)
    Task<BookResponseDto> DeleteBook(Guid bookId);
    //• Retrieve all books 
    Task<PagedResult<BookResponseDto>> ViewAllBooks(BaseQuery query);
    //• Retrieve a single book 
    Task<BookResponseDto> GetBookById(Guid bookId);
    //• Filter books by category 
    Task<PagedResult<BookResponseDto>> GetBooksByCategory(Guid categoryId, BaseQuery query);
    //• Filter books by author 
    Task<PagedResult<BookResponseDto>> GetBooksByAuthor(Guid authorId, BaseQuery query);
    //• Check book availability
    Task<BookResponseDto> IsBookAvailable(Guid bookId);
    // General Search
    Task<BookResponseDto> GetBook(string search);

}
