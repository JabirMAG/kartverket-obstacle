using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    public class RapportData
    {
        [Key]
        public int RapportID { get; set; }

        // FK to ObstacleData
        [ForeignKey(nameof(Obstacle))]
        public int ObstacleId { get; set; }

        public ObstacleData Obstacle { get; set; }

        [MaxLength(1000)]
        public string RapportComment { get; set; } = string.Empty;

        // FK to ApplicationUser (the user who reported/created this rapport)
        [ForeignKey(nameof(ReportedBy))]
        public string? ReportedByUserId { get; set; }

        public ApplicationUser? ReportedBy { get; set; }
    }
}
