using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<Advice> Feedback { get; set; }
        public DbSet<ObstacleData> ObstaclesData { get; set; }
        public DbSet<RapportData> Rapports { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Advice>().HasKey(Key => Key.adviceID);
            modelBuilder.Entity<ObstacleData>().HasKey(pri => pri.ObstacleId);
            modelBuilder.Entity<RapportData>().HasKey(r => r.RapportID);

            modelBuilder.Entity<RapportData>()
                .HasOne(r => r.Obstacle)
                .WithMany(o => o.Rapports)
                .HasForeignKey(r => r.ObstacleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}