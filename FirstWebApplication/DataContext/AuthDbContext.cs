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

            var PilotRoleId = "1";
            var AdminRoleId = "2";
            var CaseHandlerId = "3";

            var roles = new List<IdentityRole>

            {
                new IdentityRole
                {
                        Name = "Pilot",
                        NormalizedName = "PILOT",
                        Id = PilotRoleId,
                        ConcurrencyStamp = PilotRoleId
                },
                 new IdentityRole
                {
                    Name = "CaseHandler",
                    NormalizedName = "CASEHANDLER",
                    Id = CaseHandlerId,
                    ConcurrencyStamp = CaseHandlerId

                },
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Id = AdminRoleId,
                    ConcurrencyStamp = AdminRoleId
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);


            // Seed AdminUser 
            var AdminId = "2";
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
                    RoleId = CaseHandlerId,
                    UserId = AdminId
                }
            };

            builder.Entity<IdentityUserRole<string>>().HasData(AdminRoles);

        }
    }
}
