using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repositories
{
    public class ObstacleRepository : IObstacleRepository
    {
        private readonly ApplicationDBContext _context;

        public ObstacleRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<ObstacleData> AddObstacle(ObstacleData obstacle)
        {
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        public async Task<ObstacleData> GetElementById(int id)
        {
            var findById = await _context.ObstaclesData
                .Where(x => x.ObstacleId == id)
                .FirstOrDefaultAsync();

            if (findById != null)
            {
                // Håndter null-verdier fra databasen
                findById.ObstacleName = findById.ObstacleName ?? string.Empty;
                findById.ObstacleDescription = findById.ObstacleDescription ?? string.Empty;
                findById.GeometryGeoJson = findById.GeometryGeoJson ?? string.Empty;
                findById.OwnerUserId = findById.OwnerUserId ?? string.Empty;
                return findById;
            }
            else
            {
                return null;
            }
        }

        public async Task<ObstacleData> UpdateObstacles(ObstacleData obstacle)
        {
            if (obstacle.ObstacleStatus == 3)
            {
                // Status 3 betyr slett hindring
                _context.ObstaclesData.Remove(obstacle);
                await _context.SaveChangesAsync();
                return obstacle;
            }
            _context.ObstaclesData.Update(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        public async Task<IEnumerable<ObstacleData>> GetAllObstacles()
        {
            var obstacles = await _context.ObstaclesData
                .OrderByDescending(x => x.ObstacleId)
                .Take(50)
                .ToListAsync();
            
            // Håndter null-verdier fra databasen
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
                obstacle.OwnerUserId = obstacle.OwnerUserId ?? string.Empty;
            }
            
            return obstacles;
        }

        public async Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId)
        {
            var obstacles = await _context.ObstaclesData
                .Where(x => x.OwnerUserId == ownerUserId)
                .OrderByDescending(x => x.ObstacleId)
                .ToListAsync();
            
            // Håndter null-verdier fra databasen
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
                obstacle.OwnerUserId = obstacle.OwnerUserId ?? string.Empty;
            }
            
            return obstacles;
        }

        public async Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId)
        {
            return await _context.ObstaclesData
                .Where(x => x.OwnerUserId == ownerUserId)
                .OrderByDescending(x => x.ObstacleId)
                .ToListAsync();
        }
    }
}
