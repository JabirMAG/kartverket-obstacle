using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    // ViewModel for hindringsdata brukt i skjemaer. GeometryGeoJson er alltid påkrevd.
    // Andre felt er valgfrie for å støtte hurtiglagringsfunksjonalitet.
    public class ObstacleDataViewModel
    {
        public int ViewObstacleId { get; set; }

        // Navn på hindringen. Valgfri for hurtiglagring, maks 100 tegn
        [MaxLength(100)]
        public string ViewObstacleName { get; set; } = string.Empty;

        // Høyde på hindringen i meter. Valgfri for hurtiglagring, må være mellom 0 og 200 når oppgitt
        [Range(0, 200, ErrorMessage = "Height must be between 0 and 200 meters")]
        public double ViewObstacleHeight { get; set; }

        // Beskrivelse av hindringen. Valgfri for hurtiglagring, maks 1000 tegn
        [MaxLength(1000)]
        public string ViewObstacleDescription { get; set; } = string.Empty;

        // Geometrisk representasjon av hindringen i GeoJSON-format. Påkrevd felt som holder koordinatene til hindringens plassering
        [Required(ErrorMessage = "Geometry (GeoJSON) is required.")]
        public string ViewGeometryGeoJson { get; set; } = string.Empty;

        // Status på hindringen (1=Ventende, 2=Godkjent, 3=Avslått). Standard er "Ventende"
        [Range(1, 3)]
        public int ViewObstacleStatus { get; set; } = 1;
    }
}

