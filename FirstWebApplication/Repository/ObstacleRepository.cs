using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repository
{
    public class ObstacleRepository : IObstacleRepository
    {
            private readonly ApplicationDBContext _context;

            public ObstacleRepository(ApplicationDBContext context)
            {
                _context = context;
            }

            public async Task<ObstacleData> AddObstacleData(ObstacleData obstacleData)
            {
                await _context.ObstaclesData.AddAsync(obstacleData);
                await _context.SaveChangesAsync();
                return obstacleData;
            }

            
            public async Task<ObstacleData> GetElementById(int id)
            {
                var findById = await _context.ObstaclesData.Where(x => x.ObstacleId == id).FirstOrDefaultAsync();
                if (findById != null)
                {
                    return findById;
                }
                else
                {
                    return null;
                }
            } 
            public async Task<IEnumerable<ObstacleData>> GetAllObstacle(ObstacleData obstacleData)
            {
                var getAllData = await _context.ObstaclesData.Take(50).ToListAsync();
                return getAllData;
            }

            public async Task<ObstacleData> UpdateObstacle(ObstacleData obstacleData)
            {
                _context.ObstaclesData.Update(obstacleData);
                await _context.SaveChangesAsync();
                return obstacleData;
            }

        }
}
