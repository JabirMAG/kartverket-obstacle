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
        public DbSet<ArchivedReport> ArchivedReports { get; set; }
        public DbSet<ArchivedRapport> ArchivedRapports { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Advice>().HasKey(Key => Key.adviceID);
            modelBuilder.Entity<ObstacleData>().HasKey(pri => pri.ObstacleId);
            modelBuilder.Entity<RapportData>().HasKey(r => r.RapportID);
            modelBuilder.Entity<ArchivedReport>().HasKey(a => a.ArchivedReportId);
            modelBuilder.Entity<ArchivedRapport>().HasKey(ar => ar.ArchivedRapportId);

            modelBuilder.Entity<ArchivedRapport>()
                .HasOne(ar => ar.ArchivedReport)
                .WithMany(ar => ar.ArchivedRapports)
                .HasForeignKey(ar => ar.ArchivedReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RapportData>()
                           .HasOne(r => r.Obstacle)
                           .WithMany(o => o.Rapports)
                           .HasForeignKey(r => r.ObstacleId)
                           .OnDelete(DeleteBehavior.Cascade);
        }
    }
}