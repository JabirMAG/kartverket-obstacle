using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    public interface IObstacleRepository
    {
        Task<ObstacleData> AddObstacleData(ObstacleData obstacleData);

        Task<ObstacleData> GetElementById(int id);

        Task<IEnumerable<ObstacleData>> GetAllObstacle(ObstacleData obstaleData);

        Task<ObstacleData> UpdateObstacle(ObstacleData obsatcleData);

    }
}
