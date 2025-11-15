using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    // ArchivedReport representerer en arkivert hindring som har status "Rejected"
    public class ArchivedReport
    {
        [Key]
        public int ArchivedReportId { get; set; }

        // Original ObstacleId fra ObstaclesData tabellen
        public int OriginalObstacleId { get; set; }

        [MaxLength(100)]
        public string ObstacleName { get; set; } = string.Empty;

        [Range(0, 200)]
        public double ObstacleHeight { get; set; }

        [MaxLength(1000)]
        public string ObstacleDescription { get; set; } = string.Empty;

        [Required]
        public string GeometryGeoJson { get; set; } = string.Empty;

        // Status er alltid 3 (Rejected) for arkiverte rapporter
        public int ObstacleStatus { get; set; } = 3;

        // Dato for når rapporten ble arkivert
        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;

        // Alle rapport-kommentarer lagret som JSON array
        // Format: ["comment1", "comment2", ...]
        public string RapportComments { get; set; } = "[]";
    }
}

