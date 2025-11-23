using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Represents a report/comment associated with an obstacle
    /// </summary>
    public class RapportData
    {
        [Key]
        public int RapportID { get; set; }

        /// <summary>
        /// Foreign key to ObstacleData
        /// </summary>
        [ForeignKey(nameof(Obstacle))]
        public int ObstacleId { get; set; }

        public ObstacleData Obstacle { get; set; }

        /// <summary>
        /// Comment text for the report. Max 1000 characters
        /// </summary>
        [MaxLength(1000)]
        public string RapportComment { get; set; } = string.Empty;
    }
}
