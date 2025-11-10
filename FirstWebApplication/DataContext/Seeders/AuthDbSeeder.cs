using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FirstWebApplication.DataContext.Seeders
{
    public static class AuthDbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDBContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            // Seed Roles
            string[] roleNames = { "Pilot", "Admin", "Registerfører" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@kartverket.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IaApproved = true,
                    FullName = "System Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    // Only assign Admin role to the admin user
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure existing admin user only has Admin role
                var existingRoles = await userManager.GetRolesAsync(adminUser);
                if (!existingRoles.Contains("Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                // Remove any non-Admin roles
                var rolesToRemove = existingRoles.Where(r => r != "Admin").ToList();
                if (rolesToRemove.Any())
                {
                    await userManager.RemoveFromRolesAsync(adminUser, rolesToRemove);
                }
            }
        }
    }
}