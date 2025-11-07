using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext.Seeders
{
    public static class AuthDbSeeder
    {
        public static void Seed(ApplicationContext context)
        {
            // Make sure DB is created
            context.Database.EnsureCreated();

            // Seed Roles
            if (!context.Roles.Any())
            {
                var PilotRoleId = "b4b5065b-e9dc-40d4-a49d-f00d9c720e75";
                var AdminRoleId = "2de8d9c9-988c-400b-ac7d-7b45c59b6251";
                var RegisterførerRoleId = "27a609d2-154c-41bb-8257-45314e8065f2";

                var roles = new List<IdentityRole>
                {
                    new IdentityRole { Id = PilotRoleId, Name = "Pilot", NormalizedName = "PILOT" },
                    new IdentityRole { Id = AdminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Id = RegisterførerRoleId, Name = "Registerfører", NormalizedName = "REGISTERFØRER" }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // Seed Admin User (ensure exists and has only the Admin role)
            var defaultAdminEmail = "admin@kartverket.com";
            var adminRole = context.Roles.SingleOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                // Shouldn't happen because we seeded roles above, but guard anyway
                adminRole = new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" };
                context.Roles.Add(adminRole);
                context.SaveChanges();
            }

            var existingAdmin = context.Users.SingleOrDefault(u => u.Email == defaultAdminEmail);
            if (existingAdmin == null)
            {
                var adminId = "d01a810e-9587-4732-90dd-208175e61b60";
                var adminUser = new ApplicationUser
                {
                    Id = adminId,
                    UserName = defaultAdminEmail,
                    Email = defaultAdminEmail,
                    NormalizedUserName = defaultAdminEmail.ToUpperInvariant(),
                    NormalizedEmail = defaultAdminEmail.ToUpperInvariant()
                };

                adminUser.PasswordHash = new PasswordHasher<ApplicationUser>()
                    .HashPassword(adminUser, "Admin123");

                context.Users.Add(adminUser);
                context.SaveChanges();

                // Ensure only Admin role is assigned to the admin account
                var adminUserRole = new IdentityUserRole<string> { UserId = adminId, RoleId = adminRole.Id };
                context.UserRoles.Add(adminUserRole);
                context.SaveChanges();
            }
            else
            {
                // If admin exists, ensure they have Admin role and remove any extra roles
                var adminId = existingAdmin.Id;
                var userRoles = context.UserRoles.Where(ur => ur.UserId == adminId).ToList();

                // Remove roles that are not the Admin role
                var toRemove = userRoles.Where(ur => ur.RoleId != adminRole.Id).ToList();
                if (toRemove.Any())
                {
                    context.UserRoles.RemoveRange(toRemove);
                    context.SaveChanges();
                }

                // Ensure admin has the Admin role
                var hasAdmin = userRoles.Any(ur => ur.RoleId == adminRole.Id);
                if (!hasAdmin)
                {
                    context.UserRoles.Add(new IdentityUserRole<string> { UserId = adminId, RoleId = adminRole.Id });
                    context.SaveChanges();
                }
            }
        }
    }
}
