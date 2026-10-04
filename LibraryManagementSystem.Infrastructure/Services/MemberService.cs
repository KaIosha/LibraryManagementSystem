using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.DTOs.MemberDtos;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Services;

public class MemberService : IMemberService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberResponseDto> UpdateMemberAsync(string memberId, UpdateMemberDto dto)
    {
        var user = await _userManager.FindByIdAsync(memberId);

        if (user is null)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        var isMember = await _userManager.IsInRoleAsync(user, SeedRoles.Member.Name!);

        if (!isMember)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "This account is not a member."
            };
        }

        if (!user.IsActive)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member is deactivated."
            };
        }

        var userName = dto.UserName.Trim();

        var nameTaken = await _userManager.FindByNameAsync(userName);

        if (nameTaken is not null && nameTaken.Id != memberId)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "This username already exists."
            };
        }

        user.Name = dto.Name.Trim();
        user.UserName = userName;
        user.PhoneNumber = dto.PhoneNumber?.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = $"Update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }

        var borrows = await _unitOfWork._BorrowsRepo.GetByMemberIdAsync(user.Id);

        int active = 0;

        foreach (var borrow in borrows)
        {
            if (borrow.Status == BorrowStatus.Borrowed || borrow.Status == BorrowStatus.Overdue)
            {
                active++;
            }
        }

        return new MemberResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            ActiveBorrowsCount = active,
            IsSuccess = true,
            Message = "Member updated successfully."
        };
    }

    public async Task<MemberResponseDto> DeleteMemberAsync(string memberId)
    {
        var user = await _userManager.FindByIdAsync(memberId);

        if (user is null)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        var isMember = await _userManager.IsInRoleAsync(user, SeedRoles.Member.Name!);

        if (!isMember)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "This account is not a member."
            };
        }

        if (!user.IsActive)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member is already deactivated."
            };
        }

        var borrows = await _unitOfWork._BorrowsRepo.GetByMemberIdAsync(memberId);

        foreach (var borrow in borrows)
        {
            if (borrow.Status == BorrowStatus.Borrowed || borrow.Status == BorrowStatus.Overdue)
            {
                return new MemberResponseDto
                {
                    IsSuccess = false,
                    Message = "Cannot delete member with active borrows. Return books first."
                };
            }
        }

        // Soft-delete via IsActive = false: borrowing history is kept,
        // login/borrow blocked by IsActive checks in AuthService/BorrowService.
        // Revoke refresh tokens so existing sessions stop refreshing.
        var tokens = await _unitOfWork._RefreshTokenRepo.GetByUserIdAsync(memberId);

        foreach (var token in tokens)
        {
            _unitOfWork._RefreshTokenRepo.Delete(token);
        }

        await _unitOfWork.SaveChangesAsync();

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = $"Delete failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }

        return new MemberResponseDto
        {
            IsSuccess = true,
            Message = "Member deleted successfully (deactivated, history kept)."
        };
    }

    public async Task<MemberResponseDto> ReactivateMemberAsync(string memberId)
    {
        var user = await _userManager.FindByIdAsync(memberId);

        if (user is null)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        var isMember = await _userManager.IsInRoleAsync(user, SeedRoles.Member.Name!);

        if (!isMember)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "This account is not a member."
            };
        }

        if (user.IsActive)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member is already active."
            };
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = $"Reactivate failed: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }

        return new MemberResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            IsSuccess = true,
            Message = "Member reactivated successfully."
        };
    }

    public async Task<MemberResponseDto> GetMemberProfileAsync(string memberId)
    {
        var user = await _userManager.FindByIdAsync(memberId);

        if (user is null)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        var isMember = await _userManager.IsInRoleAsync(user, SeedRoles.Member.Name!);

        if (!isMember)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "This account is not a member."
            };
        }

        if (!user.IsActive)
        {
            return new MemberResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        var borrows = await _unitOfWork._BorrowsRepo.GetByMemberIdAsync(user.Id);

        int active = 0;

        foreach (var borrow in borrows)
        {
            if (borrow.Status == BorrowStatus.Borrowed || borrow.Status == BorrowStatus.Overdue)
            {
                active++;
            }
        }

        return new MemberResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            ActiveBorrowsCount = active,
            IsSuccess = true,
            Message = "Member retrieved successfully."
        };
    }

    public async Task<PagedResult<MemberBorrowedBookDto>> GetMemberBorrowsAsync(string memberId, BaseQuery query)
    {
        var empty = new PagedResult<MemberBorrowedBookDto>
        {
            Items = new(),
            TotalCount = 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        var user = await _userManager.FindByIdAsync(memberId);

        if (user is null)
        {
            return empty;
        }

        var isMember = await _userManager.IsInRoleAsync(user, SeedRoles.Member.Name!);

        if (!isMember)
        {
            return empty;
        }

        var borrows = await _unitOfWork._BorrowsRepo.GetByMemberIdAsync(memberId);

        var bookIds = new List<Guid>();

        foreach (var borrow in borrows)
        {
            bookIds.Add(borrow.BookId);
        }

        var neededBooks = await _unitOfWork._BooksRepo.GetQueryable()
            .Where(b => bookIds.Contains(b.BookId))
            .ToListAsync();

        var bookNames = new Dictionary<Guid, Book>();

        foreach (var book in neededBooks)
        {
            bookNames[book.BookId] = book;
        }

        var filtered = new List<MemberBorrowedBookDto>();

        foreach (var borrow in borrows)
        {
            bookNames.TryGetValue(borrow.BookId, out var book);

            var title = book?.Title ?? string.Empty;
            var isbn = book?.ISBN ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();

                if (!title.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    !isbn.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            filtered.Add(new MemberBorrowedBookDto
            {
                BorrowId = borrow.BorrowId,
                BookId = borrow.BookId,
                BookTitle = title,
                ISBN = isbn,
                BorrowDate = borrow.BorrowDate,
                DueDate = borrow.DueDate,
                ReturnDate = borrow.ReturnDate,
                Status = borrow.Status.ToString()
            });
        }

        var total = filtered.Count;
        var pageItems = filtered.OrderByDescending(x => x.BorrowDate).Skip(query.Skip).Take(query.Take).ToList();

        return new PagedResult<MemberBorrowedBookDto>
        {
            Items = pageItems,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
