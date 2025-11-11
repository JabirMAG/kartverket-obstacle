using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    public interface IObstacleRepository
    {
        Task<ObstacleData> AddObstacle(ObstacleData obstacles);
        Task<ObstacleData> GetElementById(int Obstacleid);
        Task<IEnumerable<ObstacleData>> GetAllObstacles();
        Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId);
        Task<ObstacleData> UpdateObstacles(ObstacleData obstacles);
        Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId);
    }
}
