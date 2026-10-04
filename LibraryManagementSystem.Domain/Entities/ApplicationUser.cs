using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? EmailConfirmationCode { get; set; }
        public DateTime? EmailConfirmationCodeExpiresAt { get; set; }= DateTime.UtcNow.AddMinutes(10);
        public int EmailConfirmationCodeAttempts { get; set; } = 0;
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiresAt { get; set; }
        public int PasswordResetCodeAttempts { get; set; } = 0;


        public ICollection<Borrow> BorrowsAsMember { get; set; } = new List<Borrow>();
        public ICollection<Borrow> BorrowsAsStaff { get; set; } = new List<Borrow>();
        public ICollection<RefreshToken> Tokens { get; set; } = new List<RefreshToken>();

    }
}
