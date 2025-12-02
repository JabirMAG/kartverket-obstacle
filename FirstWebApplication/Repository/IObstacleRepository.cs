using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for obstacle data operations
    /// </summary>
    public interface IObstacleRepository
    {
        /// <summary>
        /// Adds a new obstacle
        /// </summary>
        Task<ObstacleData> AddObstacle(ObstacleData obstacles);
        
        /// <summary>
        /// Gets an obstacle by ID
        /// </summary>
        Task<ObstacleData?> GetElementById(int Obstacleid);
        
        /// <summary>
        /// Gets all obstacles
        /// </summary>
        Task<IEnumerable<ObstacleData>> GetAllObstacles();
        
        /// <summary>
        /// Updates an existing obstacle
        /// </summary>
        Task<ObstacleData> UpdateObstacles(ObstacleData obstacles);
        
        /// <summary>
        /// Gets all obstacles owned by a specific user
        /// </summary>
        Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId);
        
        /// <summary>
        /// Gets an obstacle by ID if it exists and is owned by the specified user
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="ownerUserId">The ID of the user who should own the obstacle</param>
        /// <returns>The obstacle if it exists and is owned by the user, otherwise null</returns>
        Task<ObstacleData?> GetObstacleByOwnerAndId(int obstacleId, string ownerUserId);
        
        /// <summary>
        /// Gets all obstacle IDs owned by a specific user
        /// </summary>
        /// <param name="ownerUserId">The ID of the user</param>
        /// <returns>HashSet of obstacle IDs owned by the user</returns>
        Task<HashSet<int>> GetObstacleIdsByOwner(string ownerUserId);
        
        /// <summary>
        /// Gets all obstacles that are pending or approved (status 1 or 2)
        /// </summary>
        /// <returns>List of reported obstacles</returns>
        Task<List<ObstacleData>> GetReportedObstacles();
        
        /// <summary>
        /// Gets all pending obstacles (status 1)
        /// </summary>
        /// <returns>List of pending obstacles</returns>
        Task<List<ObstacleData>> GetPendingObstacles();
        
        /// <summary>
        /// Updates the status of an obstacle. If status is Rejected (3), the obstacle is deleted.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="status">The new status for the obstacle</param>
        /// <returns>The updated obstacle, or null if obstacle not found</returns>
        Task<ObstacleData?> UpdateObstacleStatus(int obstacleId, int status);
        
        /// <summary>
        /// Checks if an obstacle can be edited (must have pending status)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to check</param>
        /// <returns>True if obstacle can be edited, otherwise false</returns>
        Task<bool> CanEditObstacle(int obstacleId);
        
        /// <summary>
        /// Updates the properties of an obstacle (name, description, height)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="name">The new name</param>
        /// <param name="description">The new description</param>
        /// <param name="height">The new height</param>
        /// <returns>The updated obstacle, or null if obstacle not found</returns>
        Task<ObstacleData?> UpdateObstacleProperties(int obstacleId, string name, string description, double height);
        
        /// <summary>
        /// Sets the owner of an obstacle from the current user context
        /// </summary>
        /// <param name="obstacle">The obstacle to set the owner for</param>
        /// <param name="userId">The ID of the user who should own the obstacle</param>
        /// <returns>The obstacle with owner set</returns>
        ObstacleData SetObstacleOwner(ObstacleData obstacle, string userId);
        
        /// <summary>
        /// Maps ObstacleData to JSON-friendly format with lowercase property names
        /// </summary>
        /// <param name="obstacles">List of obstacles to map</param>
        /// <returns>List of anonymous objects in JSON format</returns>
        List<object> MapToJsonFormat(IEnumerable<ObstacleData> obstacles);
        
        /// <summary>
        /// Normalizes obstacle data for quick save by setting default values for optional fields
        /// </summary>
        /// <param name="obstacle">The obstacle data to normalize</param>
        /// <returns>The normalized obstacle data</returns>
        ObstacleData NormalizeForQuickSave(ObstacleData obstacle);
        
        /// <summary>
        /// Validates that GeometryGeoJson is not empty (required for quick save)
        /// </summary>
        /// <param name="obstacle">The obstacle data to validate</param>
        /// <returns>True if GeometryGeoJson is valid, false otherwise</returns>
        bool IsGeometryValid(ObstacleData obstacle);
        
        /// <summary>
        /// Maps ObstacleDataViewModel to ObstacleData entity
        /// </summary>
        /// <param name="viewModel">The ViewModel containing obstacle data</param>
        /// <returns>ObstacleData entity ready for database storage</returns>
        ObstacleData MapFromViewModel(ObstacleDataViewModel viewModel);
        
        /// <summary>
        /// Maps ObstacleData entity to ObstacleDataViewModel
        /// </summary>
        /// <param name="obstacle">The ObstacleData entity</param>
        /// <returns>ObstacleDataViewModel ready for view display</returns>
        ObstacleDataViewModel MapToViewModel(ObstacleData obstacle);
    }
}