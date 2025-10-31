using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options) { }

        public DbSet<Advice> Feedback { get; set; }
        public DbSet<ObstacleData> ObstaclesData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Advice>().HasKey(Key => Key.adviceID);
            modelBuilder.Entity<ObstacleData>().HasKey(Key => Key.ObstacleDataId);
            
        }
    }
}
