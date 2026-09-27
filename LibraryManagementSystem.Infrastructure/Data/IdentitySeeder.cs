using LibraryManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.Infrastructure.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var config = services.GetRequiredService<IConfiguration>();

        foreach (var role in SeedRoles.GetAllRoles())
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role.Name,
                    NormalizedName = role.NormalizedName,
                    Description = role.Description
                });
            }
        }

        await SeedUserAsync(
            userManager,
            config,
            section: "SeedUsers:Admin",
            defaultEmail: "admin@library.local",
            defaultUserName: "admin",
            defaultName: "Admin",
            defaultPassword: "Admin123!",
            role: SeedRoles.Librarian.Name!);

        await SeedUserAsync(
            userManager,
            config,
            section: "SeedUsers:Staff",
            defaultEmail: "staff@library.local",
            defaultUserName: "staff",
            defaultName: "Staff",
            defaultPassword: "Staff123!",
            role: SeedRoles.Staff.Name!);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        string section,
        string defaultEmail,
        string defaultUserName,
        string defaultName,
        string defaultPassword,
        string role)
    {
        var email = config[$"{section}:Email"] ?? defaultEmail;
        var userName = config[$"{section}:UserName"] ?? defaultUserName;
        var name = config[$"{section}:Name"] ?? defaultName;
        var password = config[$"{section}:Password"] ?? defaultPassword;

        var existing = await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Name = name,
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            throw new Exception(
                $"Failed to seed {section} user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}
