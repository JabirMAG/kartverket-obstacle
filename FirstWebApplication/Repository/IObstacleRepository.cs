using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    public interface IObstacleRepository
    {
        Task<ObstacleData> AddObstacle(ObstacleData obstacles);
        Task<ObstacleData> GetElementById(int Obstacleid);
        Task<IEnumerable<ObstacleData>> GetAllObstacles();
        Task<ObstacleData> DeleteById(int Obstacleid);
        Task DeleteObstacle();
        Task<ObstacleData> UpdateObstacles(ObstacleData obstacles);
    }
}
