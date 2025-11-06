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

        [Range(1, 3)]
        public int RapportStatus { get; set; } = 1;
    }
}
