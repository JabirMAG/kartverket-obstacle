using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

public static class ObstacleRepository
{
    private static List<ObstacleData> _obstacles = new List<ObstacleData>();

    public static List<ObstacleData> GetAllObstacles()
    {
        return _obstacles;
    }

    public static void AddObstacle(ObstacleData obstacle)
    {
        _obstacles.Add(obstacle);
    }
}