using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    /// <summary>
    /// Database context for the application. Extends IdentityDbContext to include custom entities
    /// </summary>
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDBContext
        /// </summary>
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        /// <summary>
        /// Database set for user feedback/advice entries
        /// </summary>
        public DbSet<Advice> Feedback { get; set; }
        
        /// <summary>
        /// Database set for obstacle data entries
        /// </summary>
        public DbSet<ObstacleData> ObstaclesData { get; set; }
        
        /// <summary>
        /// Database set for report/rapport entries
        /// </summary>
        public DbSet<RapportData> Rapports { get; set; }
        
        /// <summary>
        /// Database set for archived reports
        /// </summary>
        public DbSet<ArchivedReport> ArchivedReports { get; set; }
        

        /// <summary>
        /// Configures entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Advice>().HasKey(Key => Key.adviceID);
            modelBuilder.Entity<ObstacleData>().HasKey(pri => pri.ObstacleId);
            modelBuilder.Entity<RapportData>().HasKey(r => r.RapportID);
            modelBuilder.Entity<ArchivedReport>().HasKey(a => a.ArchivedReportId);

            modelBuilder.Entity<ObstacleData>(entity =>
            {
                entity.Property(e => e.ObstacleName).IsRequired(false);
                entity.Property(e => e.ObstacleDescription).IsRequired(false);
                entity.Property(e => e.GeometryGeoJson).IsRequired(false);
                entity.Property(e => e.OwnerUserId).IsRequired(false);
            });

            modelBuilder.Entity<RapportData>()
                           .HasOne(r => r.Obstacle)
                           .WithMany(o => o.Rapports)
                           .HasForeignKey(r => r.ObstacleId)
                           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ObstacleData>()
                .HasOne(o => o.OwnerUser)
                .WithMany()
                .HasForeignKey(o => o.OwnerUserId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}