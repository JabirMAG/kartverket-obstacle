using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for obstacle data operations
    /// </summary>
    public class ObstacleRepository : IObstacleRepository
    {
        private readonly ApplicationDBContext _context;

        public ObstacleRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new obstacle
        /// </summary>
        public async Task<ObstacleData> AddObstacle(ObstacleData obstacle)
        {
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        /// <summary>
        /// Gets an obstacle by ID
        /// </summary>
        public async Task<ObstacleData?> GetElementById(int id)
        {
            var findById = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.ObstacleId == id)
                .FirstOrDefaultAsync();

            if (findById != null)
            {
                findById.ObstacleName = findById.ObstacleName ?? string.Empty;
                findById.ObstacleDescription = findById.ObstacleDescription ?? string.Empty;
                findById.GeometryGeoJson = findById.GeometryGeoJson ?? string.Empty;
                return findById;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates an existing obstacle. If status is 3, the obstacle is deleted
        /// </summary>
        public async Task<ObstacleData> UpdateObstacles(ObstacleData obstacle)
        {
            if (obstacle.ObstacleStatus == 3)
            {
                _context.ObstaclesData.Remove(obstacle);
                await _context.SaveChangesAsync();
                return obstacle;
            }
            _context.ObstaclesData.Update(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        /// <summary>
        /// Gets the 50 most recent obstacles
        /// </summary>
        public async Task<IEnumerable<ObstacleData>> GetAllObstacles()
        {
            var obstacles = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .OrderByDescending(x => x.ObstacleId)
                .Take(50)
                .ToListAsync();
            
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }
            
            return obstacles;
        }

        /// <summary>
        /// Gets all obstacles owned by a specific user
        /// </summary>
        public async Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId)
        {
            var obstacles = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.OwnerUserId == ownerUserId)
                .OrderByDescending(x => x.ObstacleId)
                .ToListAsync();
            
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }
            
            return obstacles;
        }

        /// <summary>
        /// Gets an obstacle by ID if it exists and is owned by the specified user
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="ownerUserId">The ID of the user who should own the obstacle</param>
        /// <returns>The obstacle if it exists and is owned by the user, otherwise null</returns>
        public async Task<ObstacleData?> GetObstacleByOwnerAndId(int obstacleId, string ownerUserId)
        {
            var obstacle = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.ObstacleId == obstacleId && x.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            if (obstacle != null)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }

            return obstacle;
        }

        /// <summary>
        /// Gets all obstacle IDs owned by a specific user
        /// </summary>
        /// <param name="ownerUserId">The ID of the user</param>
        /// <returns>HashSet of obstacle IDs owned by the user</returns>
        public async Task<HashSet<int>> GetObstacleIdsByOwner(string ownerUserId)
        {
            return await _context.ObstaclesData
                .Where(x => x.OwnerUserId == ownerUserId)
                .Select(x => x.ObstacleId)
                .ToHashSetAsync();
        }

        /// <summary>
        /// Gets all obstacles that are pending or approved (status 1 or 2)
        /// </summary>
        /// <returns>List of reported obstacles</returns>
        public async Task<List<ObstacleData>> GetReportedObstacles()
        {
            var allObstacles = await GetAllObstacles();
            return allObstacles
                .Where(o => o.ObstacleStatus == 1 || o.ObstacleStatus == 2)
                .ToList();
        }

        /// <summary>
        /// Gets all pending obstacles (status 1)
        /// </summary>
        /// <returns>List of pending obstacles</returns>
        public async Task<List<ObstacleData>> GetPendingObstacles()
        {
            var allObstacles = await GetAllObstacles();
            return allObstacles
                .Where(o => o.ObstacleStatus == 1)
                .ToList();
        }

        /// <summary>
        /// Updates the status of an obstacle. If status is Rejected (3), the obstacle is deleted.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="status">The new status for the obstacle</param>
        /// <returns>The updated obstacle, or null if obstacle not found</returns>
        public async Task<ObstacleData?> UpdateObstacleStatus(int obstacleId, int status)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return null;
            }

            obstacle.ObstacleStatus = status;
            return await UpdateObstacles(obstacle);
        }

        /// <summary>
        /// Checks if an obstacle can be edited (must have pending status)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to check</param>
        /// <returns>True if obstacle can be edited, otherwise false</returns>
        public async Task<bool> CanEditObstacle(int obstacleId)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return false;
            }
            return obstacle.ObstacleStatus == 1; // StatusPending
        }

        /// <summary>
        /// Updates the properties of an obstacle (name, description, height)
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="name">The new name</param>
        /// <param name="description">The new description</param>
        /// <param name="height">The new height</param>
        /// <returns>The updated obstacle, or null if obstacle not found</returns>
        public async Task<ObstacleData?> UpdateObstacleProperties(int obstacleId, string name, string description, double height)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return null;
            }

            obstacle.ObstacleName = name;
            obstacle.ObstacleDescription = description;
            obstacle.ObstacleHeight = height;

            return await UpdateObstacles(obstacle);
        }

        /// <summary>
        /// Sets the owner of an obstacle from the current user context
        /// </summary>
        /// <param name="obstacle">The obstacle to set the owner for</param>
        /// <param name="userId">The ID of the user who should own the obstacle</param>
        /// <returns>The obstacle with owner set</returns>
        public ObstacleData SetObstacleOwner(ObstacleData obstacle, string userId)
        {
            obstacle.OwnerUserId = userId;
            return obstacle;
        }

        /// <summary>
        /// Maps ObstacleData to JSON-friendly format with lowercase property names
        /// </summary>
        /// <param name="obstacles">List of obstacles to map</param>
        /// <returns>List of anonymous objects in JSON format</returns>
        public List<object> MapToJsonFormat(IEnumerable<ObstacleData> obstacles)
        {
            return obstacles
                .Select(o => new
                {
                    id = o.ObstacleId,
                    name = o.ObstacleName,
                    height = o.ObstacleHeight,
                    description = o.ObstacleDescription,
                    status = o.ObstacleStatus,
                    geometryGeoJson = o.GeometryGeoJson
                })
                .ToList<object>();
        }

        /// <summary>
        /// Normalizes obstacle data for quick save by setting default values for optional fields
        /// </summary>
        /// <param name="obstacle">The obstacle data to normalize</param>
        /// <returns>The normalized obstacle data</returns>
        public ObstacleData NormalizeForQuickSave(ObstacleData obstacle)
        {
            obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
            obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
            return obstacle;
        }

        /// <summary>
        /// Validates that GeometryGeoJson is not empty (required for quick save)
        /// </summary>
        /// <param name="obstacle">The obstacle data to validate</param>
        /// <returns>True if GeometryGeoJson is valid, false otherwise</returns>
        public bool IsGeometryValid(ObstacleData obstacle)
        {
            return !string.IsNullOrEmpty(obstacle.GeometryGeoJson);
        }

        /// <summary>
        /// Maps ObstacleDataViewModel to ObstacleData entity
        /// </summary>
        /// <param name="viewModel">The ViewModel containing obstacle data</param>
        /// <returns>ObstacleData entity ready for database storage</returns>
        public ObstacleData MapFromViewModel(ObstacleDataViewModel viewModel)
        {
            return new ObstacleData
            {
                ObstacleId = viewModel.ViewObstacleId,
                ObstacleName = viewModel.ViewObstacleName ?? string.Empty,
                ObstacleHeight = viewModel.ViewObstacleHeight,
                ObstacleDescription = viewModel.ViewObstacleDescription ?? string.Empty,
                GeometryGeoJson = viewModel.ViewGeometryGeoJson ?? string.Empty,
                ObstacleStatus = viewModel.ViewObstacleStatus
            };
        }

        /// <summary>
        /// Maps ObstacleData entity to ObstacleDataViewModel
        /// </summary>
        /// <param name="obstacle">The ObstacleData entity</param>
        /// <returns>ObstacleDataViewModel ready for view display</returns>
        public ObstacleDataViewModel MapToViewModel(ObstacleData obstacle)
        {
            return new ObstacleDataViewModel
            {
                ViewObstacleId = obstacle.ObstacleId,
                ViewObstacleName = obstacle.ObstacleName ?? string.Empty,
                ViewObstacleHeight = obstacle.ObstacleHeight,
                ViewObstacleDescription = obstacle.ObstacleDescription ?? string.Empty,
                ViewGeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty,
                ViewObstacleStatus = obstacle.ObstacleStatus
            };
        }
    }
}
