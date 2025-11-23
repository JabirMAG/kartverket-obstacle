using FirstWebApplication.Models;

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
    }
}