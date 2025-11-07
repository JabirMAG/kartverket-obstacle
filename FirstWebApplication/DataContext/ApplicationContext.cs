using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace FirstWebApplication.DataContext
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        // Required ctor for DI and EF
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Advice> Feedback { get; set; }
        public DbSet<ObstacleData> ObstaclesData { get; set; }
        public DbSet<RapportData> Rapports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Advice>().HasKey(k => k.adviceID);
            modelBuilder.Entity<ObstacleData>().HasKey(pri => pri.ObstacleId);
            modelBuilder.Entity<RapportData>().HasKey(r => r.RapportID);

            modelBuilder.Entity<RapportData>()
                        .HasOne(r => r.Obstacle)
                        .WithMany(o => o.Rapports)
                        .HasForeignKey(r => r.ObstacleId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Design-time factory used by EF tools (dotnet ef / Package Manager Console)
    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            // Try reading connection string from appsettings.json or environment variables.
            var basePath = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("DatabaseConnection")
                       ?? config.GetConnectionString("AuthConnection")
                       ?? "Server=localhost;Port=3307;Database=KartverketDB;User=root;Password=Mammaerbest1";

            var serverVersion = new MySqlServerVersion(new Version(11, 8, 3));

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseMySql(conn, serverVersion, mySqlOptions =>
            {
                // Mirror runtime retry settings if needed
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}
