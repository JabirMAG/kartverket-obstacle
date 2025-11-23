using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
