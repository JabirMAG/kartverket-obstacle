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
                return findById;
            }
            else
            {
                return null;
            }
        }

        public async Task DeleteObstacle()
        {
            var elementToDelete = await (
                from o in _context.ObstaclesData
                where o.ObstacleStatus == 3
                select o
            ).ToListAsync();

            if (elementToDelete != null)
            {
                _context.ObstaclesData.RemoveRange(elementToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ObstacleData> DeleteById(int id)
        {
            var elementById = await _context.ObstaclesData.FindAsync(id);

            if (elementById != null)
            {
                _context.ObstaclesData.Remove(elementById);
                await _context.SaveChangesAsync();
                return elementById;
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
                _context.ObstaclesData.Remove(obstacle);
            }
            _context.ObstaclesData.Update(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        public async Task<IEnumerable<ObstacleData>> GetAllObstacles()
        {
            return await _context.ObstaclesData
                .OrderByDescending(x => x.ObstacleId)
                .Take(50)
                .ToListAsync();
        }
    }
}
