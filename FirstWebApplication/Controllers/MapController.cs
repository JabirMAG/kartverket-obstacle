using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using FirstWebApplication.Repositories;

namespace FirstWebApplication.Controllers
{
    public class MapController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public MapController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Map()
        {
            // Get all pending (status = 1) and approved (status = 2) obstacles to display on map
            var allObstacles = await _obstacleRepository.GetAllObstacles();
            var reportedObstacles = allObstacles.Where(o => o.ObstacleStatus == 1 || o.ObstacleStatus == 2).ToList();
            
            ViewBag.ReportedObstacles = reportedObstacles;
            
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingObstacles()
        {
            // Get all pending obstacles (status = 1) for checking
            var allObstacles = await _obstacleRepository.GetAllObstacles();
            var pendingObstacles = allObstacles.Where(o => o.ObstacleStatus == 1)
                .Select(o => new {
                    id = o.ObstacleId,
                    name = o.ObstacleName,
                    height = o.ObstacleHeight,
                    description = o.ObstacleDescription,
                    status = o.ObstacleStatus,
                    geometryGeoJson = o.GeometryGeoJson
                })
                .ToList();
            
            return Json(pendingObstacles);
        }
    }
}
