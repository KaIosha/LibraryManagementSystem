using LibraryManagementSystem.Application.DTOs.AuthorDtos;
using LibraryManagementSystem.Application.DTOs.CategoryDtos;
using LibraryManagementSystem.Application.DTOs.Common;

namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface IAuthorService
{
    //• Add authors
    Task<AuthorResponseDto> CreateAuthor(CreateAuthorDto dto);
    //• Edit authors
    Task<AuthorResponseDto> EditAuthor(UpdateAuthorDto dto);
    //• Delete authors
    Task<AuthorResponseDto> DeleteAuthor(Guid authorId);
    //• View all authors
    Task<PagedResult<AuthorResponseDto>> ViewAuthors(BaseQuery query);
    //• View author details
    Task<AuthorResponseDto> ViewAuthorDetails(Guid authorId);
    //• List all books written by an author
    Task<PagedResult<BookDataDto>> ViewBooksByAuthor(Guid authorId, BaseQuery query);
}
