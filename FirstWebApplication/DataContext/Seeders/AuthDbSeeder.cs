using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FirstWebApplication.DataContext.Seeders
{
    /// <summary>
    /// Seeds the database with initial roles and admin user on application startup
    /// </summary>
    public static class AuthDbSeeder
    {
        /// <summary>
        /// Seeds the database with default roles and admin user
        /// </summary>
        /// <param name="serviceProvider">The service provider to get required services</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDBContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Pilot", "Admin", "Registerfører" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

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
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                var existingRoles = await userManager.GetRolesAsync(adminUser);
                if (!existingRoles.Contains("Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                var rolesToRemove = existingRoles.Where(r => r != "Admin").ToList();
                if (rolesToRemove.Any())
                {
                    await userManager.RemoveFromRolesAsync(adminUser, rolesToRemove);
                }
            }
        }
    }
}