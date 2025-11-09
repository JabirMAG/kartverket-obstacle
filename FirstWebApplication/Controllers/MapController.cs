using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;
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
            // Get all pending obstacles (status = 1) to display on map
            var allObstacles = await _obstacleRepository.GetAllObstacles();
            var pendingObstacles = allObstacles.Where(o => o.ObstacleStatus == 1).ToList();
            
            ViewBag.PendingObstacles = pendingObstacles;
            
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingObstacles()
        {
            // Get all pending obstacles (status = 1) for checking
            var allObstacles = await _obstacleRepository.GetAllObstacles();
            var pendingObstacles = allObstacles.Where(o => o.ObstacleStatus == 1).ToList();
            
            return Json(pendingObstacles);
        }
    }
}
