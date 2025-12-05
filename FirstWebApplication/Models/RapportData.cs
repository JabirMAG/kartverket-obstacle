using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    // Representerer en rapport/kommentar tilknyttet en hindring
    public class RapportData
    {
        [Key]
        public int RapportID { get; set; }

        // Fremmednøkkel til ObstacleData
        [ForeignKey(nameof(Obstacle))]
        public int ObstacleId { get; set; }

        public ObstacleData Obstacle { get; set; }

        // Kommentartekst for rapporten. Maks 1000 tegn
        [MaxLength(1000)]
        public string RapportComment { get; set; } = string.Empty;
    }
}
