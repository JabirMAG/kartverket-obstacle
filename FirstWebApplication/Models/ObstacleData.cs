using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FirstWebApplication.Models
{
    // ObstacleData representerer data om en hindring som registreres i systemet.
    // Brukes sammen med skjemaet i ObstacleController
    public class ObstacleData
    {
        [Key]
        public int ObstacleId { get; set; }

        // Eieren (piloten) som sendte inn hindringen
        [MaxLength(255)]
        public string? OwnerUserId { get; set; }

        // Navnet på hindringen.
        // Må fylles ut og kan maks være 100 tegn.
        [Required(ErrorMessage = "The ObstacleName field is required.")]
        [MaxLength(100)]
        public string ObstacleName { get; set; } = string.Empty;

        // Høyden på hindringen i meter.
        // Må fylles ut og må være mellom 0 og 200.
        [Range (0, 200)]
        public double ObstacleHeight { get; set; }

        // En beskrivelse av hindringen.
        /// Må fylles ut og kan maks være 1000 tegn.
        [MaxLength(1000)]
        public string ObstacleDescription { get; set; } = string.Empty;

        // Geometrisk representasjon av hindringen i GeoJSON-format.
        // Felt som beholder koordinatene til hinderets lokasjon
        [Required(ErrorMessage = "Geometry (GeoJSON) is required.")]
        public string GeometryGeoJson { get; set; }

        [Range(1,3)]
        // Status for hindringen (1=Pending, 2=Approved, 3=Rejected).
        public int ObstacleStatus { get; set;} = 1; // Default til "Pending"

        public ICollection<RapportData> Rapports { get; set; } = new List<RapportData>();
    }
}
