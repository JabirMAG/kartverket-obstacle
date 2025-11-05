using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext.Seeders
{
    public static class AuthDbSeeder
    {
        public static void Seed(AuthDbContext context)
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

            // Seed Admin User
            if (!context.Users.Any())
            {
                var adminId = "d01a810e-9587-4732-90dd-208175e61b60";
                var adminUser = new ApplicationUser
                {
                    Id = adminId,
                    UserName = "admin@kartverket.no",
                    Email = "admin@kartverket.com",
                    NormalizedUserName = "ADMIN@KARTVERKET.NO",
                    NormalizedEmail = "ADMIN@KARTVERKET.NO"
                };

                adminUser.PasswordHash = new PasswordHasher<ApplicationUser>()
                    .HashPassword(adminUser, "Admin123");

                context.Users.Add(adminUser);
                context.SaveChanges();

                // Assign roles to admin
                var adminRoles = new List<IdentityUserRole<string>>
                {
                    new IdentityUserRole<string> { UserId = adminId, RoleId = "b4b5065b-e9dc-40d4-a49d-f00d9c720e75" },
                    new IdentityUserRole<string> { UserId = adminId, RoleId = "2de8d9c9-988c-400b-ac7d-7b45c59b6251" },
                    new IdentityUserRole<string> { UserId = adminId, RoleId = "27a609d2-154c-41bb-8257-45314e8065f2" }
                };

                context.UserRoles.AddRange(adminRoles);
                context.SaveChanges();
            }
        }
    }
}
