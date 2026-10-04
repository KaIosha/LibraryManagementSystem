using LibraryManagementSystem.Application.DTOs.BorrowDtos;
using LibraryManagementSystem.Application.DTOs.Common;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Services;

public class BorrowService : IBorrowService
{
    private const decimal BorrowFee = 5m;
    private const decimal LateFeePerDay = 2m;
    private const int borrowDays = 14;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public BorrowService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<BorrowResponseDto> BorrowBook(BorrowBookDto dto)
    {
        var member = await _userManager.FindByIdAsync(dto.MemberId);
        if (member is null)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Member not found."
            };
        }

        if (!member.IsActive)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Member account is not active."
            };
        }

        var book = await _unitOfWork._BooksRepo.GetByIdAsync(dto.BookId);
        if (book is null || book.IsDeleted)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Book not found."
            };
        }

        if (!book.Availability)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Book is not available."
            };
        }

        var alreadyBorrowed = await _unitOfWork._BorrowsRepo.ExistsActiveBorrowByBookAsync(dto.BookId);
        if (alreadyBorrowed)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Book is already borrowed."
            };
        }

        if (string.IsNullOrWhiteSpace(dto.StaffId))
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Staff is required."
            };
        }

        var staff = await _userManager.FindByIdAsync(dto.StaffId.Trim());
        if (staff is null)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Staff not found."
            };
        }

        var isStaff = await _userManager.IsInRoleAsync(staff, SeedRoles.Staff.Name!)
            || await _userManager.IsInRoleAsync(staff, SeedRoles.Librarian.Name!);
        if (!isStaff)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "This account is not staff."
            };
        }

        if (!staff.IsActive)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Staff account is not active."
            };
        }

        var dueDate = DateTime.UtcNow.AddDays(borrowDays);
       

        var borrow = new Borrow
        {
            BorrowId = Guid.NewGuid(),
            BookId = dto.BookId,
            MemberId = member.Id,
            StaffId = staff.Id,
            BorrowDate = DateTime.UtcNow,
            DueDate = dueDate,
            Status = BorrowStatus.Borrowed,
            TotalAmount = BorrowFee,
            LateFee = 0m
        };

        book.Availability = false;

        await _unitOfWork._BorrowsRepo.AddAsync(borrow);
        _unitOfWork._BooksRepo.Update(book);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsActiveBorrowConflict(ex))
        {
            // Parallel request won the race: filtered unique index
            // UX_Borrows_ActiveBook rejected the second active borrow.
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Book is already borrowed."
            };
        }

        return new BorrowResponseDto
        {
            BorrowId = borrow.BorrowId,
            BookId = book.BookId,
            BookTitle = book.Title,
            ISBN = book.ISBN,
            MemberId = member.Id,
            MemberName = member.Name,
            StaffId = staff.Id,
            StaffName = staff.Name,
            BorrowDate = borrow.BorrowDate,
            DueDate = borrow.DueDate,
            ReturnDate = borrow.ReturnDate,
            Status = borrow.Status.ToString(),
            IsSuccess = true,
            Message = "Book borrowed successfully.",
            Amount = borrow.TotalAmount
        };
    }

    public async Task<BorrowResponseDto> ReturnBook(Guid borrowId)
    {
        var borrow = await _unitOfWork._BorrowsRepo.GetQueryable()
            .Include(b => b.Book)
            .Include(b => b.Member)
            .Include(b => b.Staff)
            .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

        if (borrow is null)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Borrow record not found."
            };
        }

        if (borrow.Status == BorrowStatus.Returned)
        {
            return new BorrowResponseDto
            {
                BorrowId = borrow.BorrowId,
                BookId = borrow.BookId,
                Status = borrow.Status.ToString(),
                IsSuccess = false,
                Message = "Book is already returned."
            };
        }

        if (borrow.Status == BorrowStatus.Lost)
        {
            return new BorrowResponseDto
            {
                BorrowId = borrow.BorrowId,
                BookId = borrow.BookId,
                Status = borrow.Status.ToString(),
                IsSuccess = false,
                Message = "Book is marked as lost."
            };
        }

        borrow.ReturnDate = DateTime.UtcNow;
        borrow.Status = BorrowStatus.Returned;

        if (borrow.ReturnDate > borrow.DueDate)
        {
            var daysLate = (int)Math.Ceiling((borrow.ReturnDate.Value - borrow.DueDate).TotalDays);
            if (daysLate < 1) daysLate = 1;
            borrow.LateFee = daysLate * LateFeePerDay;
        }

        var book = await _unitOfWork._BooksRepo.GetByIdAsync(borrow.BookId);
        if (book is not null && !book.IsDeleted)
        {
            book.Availability = true;
        }

        borrow.Book = null!;
        borrow.Member = null!;
        borrow.Staff = null!;

        _unitOfWork._BorrowsRepo.Update(borrow);
        await _unitOfWork.SaveChangesAsync();

        return ToResponseDto(borrow, true, "Book returned successfully.");
    }

    public async Task<BorrowResponseDto> GetBorrowById(Guid borrowId)
    {
        var borrow = await _unitOfWork._BorrowsRepo.GetQueryable()
            .Include(b => b.Book)
            .Include(b => b.Member)
            .Include(b => b.Staff)
            .FirstOrDefaultAsync(b => b.BorrowId == borrowId);

        if (borrow is null)
        {
            return new BorrowResponseDto
            {
                IsSuccess = false,
                Message = "Borrow record not found."
            };
        }

        return ToResponseDto(borrow, true, "Borrow record retrieved successfully.");
    }

    public async Task<PagedResult<BorrowResponseDto>> ViewAllBorrows(BaseQuery query)
    {
        IQueryable<Borrow> borrowsQuery = _unitOfWork._BorrowsRepo.GetQueryable()
            .Include(b => b.Book)
            .Include(b => b.Member)
            .Include(b => b.Staff);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            borrowsQuery = borrowsQuery.Where(b =>
                b.Book.Title.StartsWith(term) ||
                b.Book.ISBN.StartsWith(term) ||
                b.Member.Name.StartsWith(term));
        }

        var total = await borrowsQuery.CountAsync();

        var items = await borrowsQuery
            .OrderByDescending(b => b.BorrowDate)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BorrowResponseDto
            {
                BorrowId = b.BorrowId,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                ISBN = b.Book.ISBN,
                MemberId = b.MemberId,
                MemberName = b.Member.Name,
                StaffId = b.StaffId,
                StaffName = b.Staff != null ? b.Staff.Name : string.Empty,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status.ToString(),
                IsSuccess = true,
                Message = "Borrow record retrieved successfully.",
                Amount = b.TotalAmount
            })
            .ToListAsync();

        return new PagedResult<BorrowResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<PagedResult<BorrowResponseDto>> GetOverdueBorrows(BaseQuery query)
    {
        var now = DateTime.UtcNow;

        IQueryable<Borrow> borrowsQuery = _unitOfWork._BorrowsRepo.GetQueryable()
            .Include(b => b.Book)
            .Include(b => b.Member)
            .Include(b => b.Staff)
              .Where(b =>
            b.DueDate < now &&
            b.ReturnDate == null &&
            b.Status != BorrowStatus.Lost
        );

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            borrowsQuery = borrowsQuery.Where(b =>
                b.Book.Title.StartsWith(term) ||
                b.Book.ISBN.StartsWith(term) ||
                b.Member.Name.StartsWith(term));
        }

        var total = await borrowsQuery.CountAsync();

        var items = await borrowsQuery
            .OrderBy(b => b.DueDate)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BorrowResponseDto
            {
                BorrowId = b.BorrowId,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                ISBN = b.Book.ISBN,
                MemberId = b.MemberId,
                MemberName = b.Member.Name,
                StaffId = b.StaffId,
                StaffName = b.Staff != null ? b.Staff.Name : string.Empty,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status.ToString(),
                IsSuccess = true,
                Message = "Borrow record retrieved successfully.",
                Amount = b.TotalAmount
            })
            .ToListAsync();

        return new PagedResult<BorrowResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<PagedResult<BorrowResponseDto>> GetBorrowsByMember(string memberId, BaseQuery query)
    {
        var empty = new PagedResult<BorrowResponseDto>
        {
            Items = new(),
            TotalCount = 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        var member = await _userManager.FindByIdAsync(memberId);
        if (member is null)
        {
            throw new KeyNotFoundException("Member not found.");
        }

        var isMember = await _userManager.IsInRoleAsync(member, SeedRoles.Member.Name!);
        if (!isMember)
        {
            throw new KeyNotFoundException("This account is not a member.");
        }

        IQueryable<Borrow> borrowsQuery = _unitOfWork._BorrowsRepo.GetQueryable()
            .Include(b => b.Book)
            .Include(b => b.Member)
            .Include(b => b.Staff)
            .Where(b => b.MemberId == memberId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            borrowsQuery = borrowsQuery.Where(b =>
                b.Book.Title.StartsWith(term) ||
                b.Book.ISBN.StartsWith(term));
        }

        var total = await borrowsQuery.CountAsync();

        var items = await borrowsQuery
            .OrderByDescending(b => b.BorrowDate)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(b => new BorrowResponseDto
            {
                BorrowId = b.BorrowId,
                BookId = b.BookId,
                BookTitle = b.Book.Title,
                ISBN = b.Book.ISBN,
                MemberId = b.MemberId,
                MemberName = b.Member.Name,
                StaffId = b.StaffId,
                StaffName = b.Staff != null ? b.Staff.Name : string.Empty,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status.ToString(),
                IsSuccess = true,
                Message = "Borrow record retrieved successfully.",
                Amount = b.TotalAmount
            })
            .ToListAsync();

        return new PagedResult<BorrowResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    private static bool IsActiveBorrowConflict(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            
            if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                return true;
        }

        return ex.InnerException?.Message.Contains("UX_Borrows_ActiveBook", StringComparison.OrdinalIgnoreCase) == true;
    }
    private static BorrowResponseDto ToResponseDto(Borrow borrow, bool isSuccess, string message)
    {
        return new BorrowResponseDto
        {
            BorrowId = borrow.BorrowId,
            BookId = borrow.BookId,
            BookTitle = borrow.Book?.Title ?? string.Empty,
            ISBN = borrow.Book?.ISBN ?? string.Empty,
            MemberId = borrow.MemberId,
            MemberName = borrow.Member?.Name ?? string.Empty,
            StaffId = borrow.StaffId,
            StaffName = borrow.Staff?.Name ?? string.Empty,
            BorrowDate = borrow.BorrowDate,
            DueDate = borrow.DueDate,
            ReturnDate = borrow.ReturnDate,
            Status = borrow.Status.ToString(),
            IsSuccess = isSuccess,
            Message = message
        };
    }
}
