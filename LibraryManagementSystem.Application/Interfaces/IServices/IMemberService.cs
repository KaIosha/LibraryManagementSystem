using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.DTOs.MemberDtos;

namespace LibraryManagementSystem.Application.Interfaces.IServices;

public interface IMemberService
{
    //• Update member information
    Task<MemberResponseDto> UpdateMemberAsync(string memberId, UpdateMemberDto dto);
    //• Delete members
    Task<MemberResponseDto> DeleteMemberAsync(string memberId);
    //• Reactivate a deactivated member (Admin only)
    Task<MemberResponseDto> ReactivateMemberAsync(string memberId);
    //• View member profile
    Task<MemberResponseDto> GetMemberProfileAsync(string memberId);
    //• List all borrowed books for a member
    Task<PagedResult<MemberBorrowedBookDto>> GetMemberBorrowsAsync(string memberId, BaseQuery query);
}
