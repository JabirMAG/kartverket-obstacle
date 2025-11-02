using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    public class ObstacleDataViewModel
    {
        [Key]
        public int ViewObstacleId { get; set; }

        // Navnet på hindringen.
        // Må fylles ut og kan maks være 100 tegn.
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public string ViewObstacleName { get; set; } = string.Empty;

        // Høyden på hindringen i meter.
        // Må fylles ut og må være mellom 0 og 200.
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public double ViewObstacleHeight { get; set; }

        // En beskrivelse av hindringen.
        /// Må fylles ut og kan maks være 1000 tegn.
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public string ViewObstacleDescription { get; set; } = string.Empty;

        // Geometrisk representasjon av hindringen i GeoJSON-format.
        // Felt som beholder koordinatene til hinderets lokasjon
        public string? ViewGeometryGeoJson { get; set; }

    }
}


