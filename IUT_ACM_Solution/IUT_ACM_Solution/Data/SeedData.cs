using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;

namespace IUT_ACM_Solution.Data
{
    public class SeedData
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));
        }

        public static async Task SeedAdminUserAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRolesAsync(roleManager);

            string adminEmail = "NewAdmin@iutcessa.com";
            string adminPassword = "NewAdmin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    await userManager.AddToRoleAsync(adminUser, "Moderator");
                }
                else
                {
                    throw new Exception("Failed to create default Admin user: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
