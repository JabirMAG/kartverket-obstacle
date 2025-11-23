using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    /// <summary>
    /// Represents obstacle data registered in the system. Used with the form in ObstacleController
    /// </summary>
    public class ObstacleData
    {
        [Key]
        public int ObstacleId { get; set; }

        /// <summary>
        /// Owner (pilot) who submitted the obstacle
        /// </summary>
        [MaxLength(255)]
        public string? OwnerUserId { get; set; }

        /// <summary>
        /// Navigation property to the owner
        /// </summary>
        [ForeignKey(nameof(OwnerUserId))]
        public ApplicationUser? OwnerUser { get; set; }

        /// <summary>
        /// Name of the obstacle. Required field, max 100 characters
        /// </summary>
        [Required(ErrorMessage = "The ObstacleName field is required.")]
        [MaxLength(100)]
        public string? ObstacleName { get; set; } = string.Empty;

        /// <summary>
        /// Height of the obstacle in meters. Must be between 0 and 200
        /// </summary>
        [Range (0, 200)]
        public double ObstacleHeight { get; set; }

        /// <summary>
        /// Description of the obstacle. Max 1000 characters
        /// </summary>
        [MaxLength(1000)]
        public string? ObstacleDescription { get; set; } = string.Empty;

        /// <summary>
        /// Geometric representation of the obstacle in GeoJSON format. Field that holds the coordinates of the obstacle's location
        /// </summary>
        [Required(ErrorMessage = "Geometry (GeoJSON) is required.")]
        public string GeometryGeoJson { get; set; } = string.Empty;

        /// <summary>
        /// Status of the obstacle (1=Pending, 2=Approved, 3=Rejected). Default is "Pending"
        /// </summary>
        [Range(1,3)]
        public int ObstacleStatus { get; set;} = 1;

        public ICollection<RapportData> Rapports { get; set; } = new List<RapportData>();
    }
}
