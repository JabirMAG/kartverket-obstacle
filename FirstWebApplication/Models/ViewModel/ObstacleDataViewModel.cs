using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for obstacle data used in forms. GeometryGeoJson is always required.
    /// Other fields are optional to support quick save functionality.
    /// </summary>
    public class ObstacleDataViewModel
    {
        public int ViewObstacleId { get; set; }

        /// <summary>
        /// Name of the obstacle. Optional for quick save, max 100 characters
        /// </summary>
        [MaxLength(100)]
        public string ViewObstacleName { get; set; } = string.Empty;

        /// <summary>
        /// Height of the obstacle in meters. Optional for quick save, must be between 0 and 200 when provided
        /// </summary>
        [Range(0, 200, ErrorMessage = "Height must be between 0 and 200 meters")]
        public double ViewObstacleHeight { get; set; }

        /// <summary>
        /// Description of the obstacle. Optional for quick save, max 1000 characters
        /// </summary>
        [MaxLength(1000)]
        public string ViewObstacleDescription { get; set; } = string.Empty;

        /// <summary>
        /// Geometric representation of the obstacle in GeoJSON format. Required field that holds the coordinates of the obstacle's location
        /// </summary>
        [Required(ErrorMessage = "Geometry (GeoJSON) is required.")]
        public string ViewGeometryGeoJson { get; set; } = string.Empty;

        /// <summary>
        /// Status of the obstacle (1=Pending, 2=Approved, 3=Rejected). Default is "Pending"
        /// </summary>
        [Range(1, 3)]
        public int ViewObstacleStatus { get; set; } = 1;
    }
}


