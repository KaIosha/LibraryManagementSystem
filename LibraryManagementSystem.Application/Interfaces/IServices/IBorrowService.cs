using LibraryManagementSystem.Application.DTOs.BorrowDtos;
using LibraryManagementSystem.Application.DTOs.Common;

namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface IBorrowService
{
    //• Borrow a book
    Task<BorrowResponseDto> BorrowBook(BorrowBookDto dto);
    //• Return a borrowed book
    Task<BorrowResponseDto> ReturnBook(Guid borrowId);
    //• Retrieve a single borrow record
    Task<BorrowResponseDto> GetBorrowById(Guid borrowId);
    //• Retrieve all borrow records
    Task<PagedResult<BorrowResponseDto>> ViewAllBorrows(BaseQuery query);
    //• List overdue borrows
    Task<PagedResult<BorrowResponseDto>> GetOverdueBorrows(BaseQuery query);
    //• List borrows for a member
    Task<PagedResult<BorrowResponseDto>> GetBorrowsByMember(string memberId, BaseQuery query);
}
