using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Borrow>()
                 .HasOne(b => b.Book)
                 .WithMany(bk => bk.Borrows)
                 .HasForeignKey(b => b.BookId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Borrow)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BorrowId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Borrow>()
                .HasOne(b => b.Member)
                .WithMany(bk => bk.BorrowsAsMember)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Borrow>()
               .HasOne(b => b.Staff)
               .WithMany(bk => bk.BorrowsAsStaff)
               .HasForeignKey(b => b.StaffId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefreshToken>()
              .HasOne(r => r.User).WithMany(u => u.Tokens)
              .HasForeignKey(r => r.UserId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Book>()
            .HasOne(b => b.Author).WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .Property(x => x.Amount).HasPrecision(18, 2);

            builder.Entity<Borrow>()
                .Property(x => x.TotalAmount).HasPrecision(18, 2);

            builder.Entity<Borrow>()
                .Property(x => x.LateFee).HasPrecision(18, 2);

            builder.Entity<Book>()
                .HasIndex(x => x.ISBN).IsUnique();

            // Concurrency guard: at most one active borrow per book.
            // Borrowed = 0, Overdue = 2 (see BorrowStatus).
            builder.Entity<Borrow>()
                .HasIndex(b => b.BookId)
                .IsUnique()
                .HasDatabaseName("UX_Borrows_ActiveBook")
                .HasFilter("[Status] IN (0, 2)");

            // NOTE: no global query filter for Book.IsDeleted on purpose.
            // Borrow history must still join deleted books, so book catalog
            // queries filter !IsDeleted explicitly (see BookService/BookRepository).
            // IsDeleted = source of truth for delete; Availability = false
            // alone means borrowed OR deleted.

            //seed Roles in db
            SeedIdentityRoles(builder);
        }
     
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        private void SeedIdentityRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationRole>().HasData(SeedRoles.GetAllRoles().ToArray());
        }
    }
}
