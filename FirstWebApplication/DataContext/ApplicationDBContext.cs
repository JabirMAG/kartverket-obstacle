using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.DataContext
{
    /// <summary>
    /// Databasekontekst for applikasjonen. Utvider IdentityDbContext for å inkludere egendefinerte entiteter
    /// </summary>
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initialiserer en ny instans av ApplicationDBContext
        /// </summary>
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        /// <summary>
        /// Databsett for brukerfeedback/råd-innslag
        /// </summary>
        public DbSet<Advice> Feedback { get; set; }
        
        /// <summary>
        /// Databsett for hinderdata-innslag
        /// </summary>
        public DbSet<ObstacleData> ObstaclesData { get; set; }
        
        /// <summary>
        /// Databsett for rapport-innslag
        /// </summary>
        public DbSet<RapportData> Rapports { get; set; }
        
        /// <summary>
        /// Databsett for arkiverte rapporter
        /// </summary>
        public DbSet<ArchivedReport> ArchivedReports { get; set; }
        

        /// <summary>
        /// Konfigurerer entitetsrelasjoner og restriksjoner
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