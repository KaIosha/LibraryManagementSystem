using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Borrow> BorrowsAsMember { get; set; } = new List<Borrow>();
        public ICollection<Borrow> BorrowsAsStaff { get; set; } = new List<Borrow>();
        public ICollection<RefreshToken> Tokens { get; set; } = new List<RefreshToken>();

    }
}
