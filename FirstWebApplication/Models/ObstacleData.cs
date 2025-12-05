using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    // Representerer hindringsdata registrert i systemet. Brukes med skjemaet i ObstacleController
    public class ObstacleData
    {
        [Key]
        public int ObstacleId { get; set; }

        // Eier (pilot) som sendte inn hindringen
        [MaxLength(255)]
        public string? OwnerUserId { get; set; }

        // Navigasjonsegenskap til eieren
        [ForeignKey(nameof(OwnerUserId))]
        public ApplicationUser? OwnerUser { get; set; }

        // Navn på hindringen. Påkrevd felt, maks 100 tegn
        [Required(ErrorMessage = "The ObstacleName field is required.")]
        [MaxLength(100)]
        public string? ObstacleName { get; set; } = string.Empty;

        // Høyde på hindringen i meter. Må være mellom 0 og 200
        [Range (0, 200)]
        public double ObstacleHeight { get; set; }

        // Beskrivelse av hindringen. Maks 1000 tegn
        [MaxLength(1000)]
        public string? ObstacleDescription { get; set; } = string.Empty;

        // Geometrisk representasjon av hindringen i GeoJSON-format. Felt som holder koordinatene til hindringens plassering
        [Required(ErrorMessage = "Geometry (GeoJSON) is required.")]
        public string GeometryGeoJson { get; set; } = string.Empty;

        // Status på hindringen (1=Ventende, 2=Godkjent, 3=Avslått). Standard er "Ventende"
        [Range(1,3)]
        public int ObstacleStatus { get; set;} = 1;

        public ICollection<RapportData> Rapports { get; set; } = new List<RapportData>();
    }
}
