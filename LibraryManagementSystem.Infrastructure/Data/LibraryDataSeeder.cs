using LibraryManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.Infrastructure.Data;

public static class LibraryDataSeeder
{
    // Fixed IDs so Postman tests can use them directly (see testendpoint.txt)
    public static readonly Guid CategorySciFiId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CategoryProgrammingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CategoryHistoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid AuthorAsimovId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid AuthorOrwellId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid AuthorMartinId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static readonly Guid BookFoundationId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid BookIRobotId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid Book1984Id = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid BookCleanCodeId = Guid.Parse("10000000-0000-0000-0000-000000000004");

    public const string TestMemberId = "d0000000-0000-0000-0000-000000000001";
    public const string TestMemberEmail = "member@test.com";
    public const string TestMemberPassword = "Member123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var categories = new List<Category>
        {
            new() { CategoryId = CategorySciFiId, Name = "Science Fiction", Description = "Sci-fi novels and stories" },
            new() { CategoryId = CategoryProgrammingId, Name = "Programming", Description = "Software development books" },
            new() { CategoryId = CategoryHistoryId, Name = "History", Description = "Historical books" }
        };

        foreach (var category in categories)
        {
            if (!await dbContext.Categories.AnyAsync(c => c.CategoryId == category.CategoryId))
            {
                await dbContext.Categories.AddAsync(category);
            }
        }

        var authors = new List<Author>
        {
            new() { AuthorId = AuthorAsimovId, Name = "Isaac Asimov", Email = "asimov@test.com", Nationality = "American" },
            new() { AuthorId = AuthorOrwellId, Name = "George Orwell", Email = "orwell@test.com", Nationality = "British" },
            new() { AuthorId = AuthorMartinId, Name = "Robert C. Martin", Email = "unclebob@test.com", Nationality = "American" }
        };

        foreach (var author in authors)
        {
            if (!await dbContext.Authors.AnyAsync(a => a.AuthorId == author.AuthorId))
            {
                await dbContext.Authors.AddAsync(author);
            }
        }

        await dbContext.SaveChangesAsync();

        var books = new List<Book>
        {
            new() { BookId = BookFoundationId, Title = "Foundation", ISBN = "9780553293357", Genre = "Science Fiction", Language = "English", Availability = true, AuthorId = AuthorAsimovId, CategoryId = CategorySciFiId },
            new() { BookId = BookIRobotId, Title = "I, Robot", ISBN = "9780553294385", Genre = "Science Fiction", Language = "English", Availability = true, AuthorId = AuthorAsimovId, CategoryId = CategorySciFiId },
            new() { BookId = Book1984Id, Title = "1984", ISBN = "9780451524935", Genre = "Dystopian", Language = "English", Availability = true, AuthorId = AuthorOrwellId, CategoryId = CategoryHistoryId },
            new() { BookId = BookCleanCodeId, Title = "Clean Code", ISBN = "9780132350884", Genre = "Programming", Language = "English", Availability = true, AuthorId = AuthorMartinId, CategoryId = CategoryProgrammingId }
        };

        foreach (var book in books)
        {
            if (!await dbContext.Books.AnyAsync(b => b.BookId == book.BookId || b.ISBN == book.ISBN))
            {
                await dbContext.Books.AddAsync(book);
            }
        }

        await dbContext.SaveChangesAsync();

        var existingMember = await userManager.FindByEmailAsync(TestMemberEmail);
        if (existingMember is null)
        {
            var member = new ApplicationUser
            {
                Id = TestMemberId,
                Name = "Test Member",
                UserName = "testmember",
                Email = TestMemberEmail,
                EmailConfirmed = true,
                IsActive = true,
                PhoneNumber = "01001234567"
            };

            var createResult = await userManager.CreateAsync(member, TestMemberPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception(
                    $"Failed to seed test member: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(member, SeedRoles.Member.Name!);
        }

        // Seed one active borrow so borrow endpoints have data to return.
        // Idempotent: skipped if the book already has an active borrow.
        var hasActiveBorrow = await dbContext.Borrows.AnyAsync(b =>
            b.BookId == BookFoundationId &&
            (b.Status == Domain.Enums.BorrowStatus.Borrowed ||
             b.Status == Domain.Enums.BorrowStatus.Overdue));

        if (!hasActiveBorrow)
        {
            var staffUser = await userManager.FindByEmailAsync("staff@library.local");
            var book = await dbContext.Books.FirstOrDefaultAsync(b => b.BookId == BookFoundationId);

            if (staffUser is not null && book is not null && !book.IsDeleted && book.Availability)
            {
                var now = DateTime.UtcNow;
                await dbContext.Borrows.AddAsync(new Borrow
                {
                    BorrowId = Guid.NewGuid(),
                    BookId = BookFoundationId,
                    MemberId = TestMemberId,
                    StaffId = staffUser.Id,
                    BorrowDate = now,
                    DueDate = now.AddDays(14),
                    Status = Domain.Enums.BorrowStatus.Borrowed
                });

                book.Availability = false;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
