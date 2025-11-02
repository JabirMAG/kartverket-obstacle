using System.ComponentModel.DataAnnotations;
namespace FirstWebApplication.Models
{
    // ObstacleData representerer data om en hindring som registreres i systemet.
    // Brukes sammen med skjemaet i ObstacleController
    public class ObstacleData
    {
        public int ObstacleId { get; set; }

        // Navnet på hindringen.
        // Må fylles ut og kan maks være 100 tegn.
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
        public string? GeometryGeoJson { get; set; }


    }
}
