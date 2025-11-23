using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for obstacle data used in forms
    /// </summary>
    public class ObstacleDataViewModel
    {
        public int ViewObstacleId { get; set; }

        /// <summary>
        /// Name of the obstacle. Required field, max 100 characters
        /// </summary>
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public string ViewObstacleName { get; set; } = string.Empty;

        /// <summary>
        /// Height of the obstacle in meters. Required field, must be between 0 and 200
        /// </summary>
        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public double ViewObstacleHeight { get; set; }

        /// <summary>
        /// Description of the obstacle. Required field, max 1000 characters
        /// </summary>
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public string ViewObstacleDescription { get; set; } = string.Empty;

        /// <summary>
        /// Geometric representation of the obstacle in GeoJSON format. Field that holds the coordinates of the obstacle's location
        /// </summary>
        public string ViewGeometryGeoJson { get; set; }

        public int ViewObstacleStatus { get; set; }

    }
}


