using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    public class AuthDbContext : IdentityDbContext //Arver fra innebygd databasekontekst for brukere    
    {
        public AuthDbContext(DbContextOptions options) : base(options) // Mottar databaseinnstillinger (connectionstring, provider osv.) fra Program.cs via dependency injection
        {
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // Seed Roles (Pilot, Registerfører, Admin)

            var PilotRoleId = "b4b5065b-e9dc-40d4-a49d-f00d9c720e75";
            var AdminRoleId = "2de8d9c9-988c-400b-ac7d-7b45c59b6251";
            var RegisterførerRoleId = "27a609d2-154c-41bb-8257-45314e8065f2";

            var roles = new List<IdentityRole>

            {
                new IdentityRole
                {
                        Name = "Pilot",
                        NormalizedName = "Pilot",
                        Id = PilotRoleId,
                        ConcurrencyStamp = PilotRoleId
                },
                 new IdentityRole
                {
                    Name = "Registerfører",
                    NormalizedName = "Registerfører",
                    Id = RegisterførerRoleId,
                    ConcurrencyStamp = RegisterførerRoleId

                },
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "Admin",
                    Id = AdminRoleId,
                    ConcurrencyStamp = AdminRoleId
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);


            // Seed AdminUser 
            var AdminId = "d01a810e-9587-4732-90dd-208175e61b60";
            var AdminUser = new IdentityUser
            {
                UserName = "admin@kartverket.no",
                Email = "admin@kartverket.com",
                NormalizedEmail = "admin@kartverket.no".ToUpper(),
                NormalizedUserName = "admin@kartverket.no".ToUpper(),
                Id = AdminId
            };

            AdminUser.PasswordHash = new PasswordHasher<IdentityUser>()
                .HashPassword(AdminUser, "Admin123");

            builder.Entity<IdentityUser>().HasData(AdminUser);

            // Add all roles to Admin
            var AdminRoles = new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string>
                {
                    RoleId = PilotRoleId,
                    UserId = AdminId
                },
                new IdentityUserRole<string>
                {
                    RoleId = AdminRoleId,
                    UserId = AdminId
                },
                new IdentityUserRole<string>
                {
                    RoleId = RegisterførerRoleId,
                    UserId = AdminId
                }
            };

            builder.Entity<IdentityUserRole<string>>().HasData(AdminRoles);

        }
    }
}
