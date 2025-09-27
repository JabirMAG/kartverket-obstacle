using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class ObstacleData
    {
        [Required(ErrorMessage = "Obstacle Name is required.")]
        public string ObstacleName { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Height must be a positive number.")]
        public double ObstacleHeight { get; set; }

        public string ObstacleDescription { get; set; }

        // Felt som beholder koordinatene til hinderets lokasjon
        public string? GeometryGeoJson { get; set; }
    }
}
