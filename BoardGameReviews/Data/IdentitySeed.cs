using BoardGameReviews.Models;
using Microsoft.AspNetCore.Identity;

namespace BoardGameReviews.Data;

public static class IdentitySeed
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        if (!await roleManager.RoleExistsAsync(UserRole))
        {
            await roleManager.CreateAsync(new IdentityRole(UserRole));
        }

        var adminEmail = configuration["IdentitySeed:AdminEmail"] ?? "admin@boardgamereviews.local";
        var adminUsername = configuration["IdentitySeed:AdminUsername"] ?? "admin";
        var adminPassword = configuration["IdentitySeed:AdminPassword"] ?? "Admin123!";
        var adminOib = configuration["IdentitySeed:AdminOIB"] ?? "12345678901";
        var adminJmbg = configuration["IdentitySeed:AdminJMBG"] ?? "1234567890123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                OIB = adminOib,
                JMBG = adminJmbg
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }

        if (!await userManager.IsInRoleAsync(adminUser, UserRole))
        {
            await userManager.AddToRoleAsync(adminUser, UserRole);
        }

        var defaultUserEmail = configuration["IdentitySeed:DefaultUserEmail"] ?? "user@boardgamereviews.local";
        var defaultUsername = configuration["IdentitySeed:DefaultUsername"] ?? "user";
        var defaultUserPassword = configuration["IdentitySeed:DefaultUserPassword"] ?? "User123!";
        var defaultUserOib = configuration["IdentitySeed:DefaultUserOIB"] ?? "10987654321";
        var defaultUserJmbg = configuration["IdentitySeed:DefaultUserJMBG"] ?? "1098765432109";

        var defaultUser = await userManager.FindByEmailAsync(defaultUserEmail);
        if (defaultUser == null)
        {
            defaultUser = new AppUser
            {
                UserName = defaultUsername,
                Email = defaultUserEmail,
                EmailConfirmed = true,
                OIB = defaultUserOib,
                JMBG = defaultUserJmbg
            };

            var createDefaultUserResult = await userManager.CreateAsync(defaultUser, defaultUserPassword);
            if (!createDefaultUserResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(defaultUser, UserRole))
        {
            await userManager.AddToRoleAsync(defaultUser, UserRole);
        }
    }
}
