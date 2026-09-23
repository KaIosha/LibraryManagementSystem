
using LibraryManagementSystem.Domain.Entities;
namespace LibraryManagementSystem.Infrastructure.Data;

public static class SeedRoles
{

    public static readonly ApplicationRole Librarian = new ApplicationRole
    {
        Id = "c2b1a8d4-5f6e-7a8b-9c0d-1e2f3a4b5c6d",
        Name = "Librarian",
        NormalizedName = "LIBRARIAN",
        Description = "Manager for the Library",
        ConcurrencyStamp = "e4f3d2c1-0b9a-8f7e-6d5c-4b3a2f1e0d9c"
    };

    public static readonly ApplicationRole Staff = new ApplicationRole
    {
        Id = "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
        Name = "Staff",
        NormalizedName = "STAFF",
        Description = "Library Staff Personnel",
        ConcurrencyStamp = "b9a8f7e6-d5c4-3b2a-1f0e-9d8c7b6a5f4e"
    };

    public static readonly ApplicationRole Member = new ApplicationRole
    {
        Id = "f8e7d6c5-b4a3-2f1e-0d9c-8b7a6f5e4d3c",
        Name = "Member",
        NormalizedName = "MEMBER",
        Description = "Library Regular Member or Reader",
        ConcurrencyStamp = "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
    };

   
    public static List<ApplicationRole> GetAllRoles()
    {
        return new List<ApplicationRole> { Librarian, Staff, Member };
    }
}

