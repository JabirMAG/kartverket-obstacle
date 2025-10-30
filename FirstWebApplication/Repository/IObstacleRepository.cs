using FirstWebApplication.Models;

namespace FirstWebApplication.Repository;

public interface IObstacleRepository
{
    Task<ObstacleData> AddObstacleData(ObstacleData obstacleData);

    // Task<ObstacleData> GetElementById(int id);
    
    
    Task<IEnumerable<ObstacleData>> GetAllAdvice(ObstacleData obstacleData);

    // Task<ObstacleData> DeleteById(int id);

    Task<ObstacleData> UpdateAdvice(ObstacleData obsatcleData);
}