using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; } = string.Empty;
    }
}
