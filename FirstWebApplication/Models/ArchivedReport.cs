using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Represents an archived obstacle that has status "Rejected"
    /// </summary>
    public class ArchivedReport
    {
        [Key]
        public int ArchivedReportId { get; set; }

        /// <summary>
        /// Original ObstacleId from ObstaclesData table
        /// </summary>
        public int OriginalObstacleId { get; set; }

        [MaxLength(100)]
        public string ObstacleName { get; set; } = string.Empty;

        [Range(0, 200)]
        public double ObstacleHeight { get; set; }

        [MaxLength(1000)]
        public string ObstacleDescription { get; set; } = string.Empty;

        [Required]
        public string GeometryGeoJson { get; set; } = string.Empty;

        /// <summary>
        /// Status is always 3 (Rejected) for archived reports
        /// </summary>
        public int ObstacleStatus { get; set; } = 3;

        /// <summary>
        /// Date when the report was archived
        /// </summary>
        public DateTime ArchivedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// All report comments stored as JSON array. Format: ["comment1", "comment2", ...]
        /// </summary>
        public string RapportComments { get; set; } = "[]";
    }
}

