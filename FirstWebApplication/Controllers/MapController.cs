using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for map functionality. Handles display of obstacles on the map.
    /// </summary>
    public class MapController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public MapController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        /// <summary>
        /// Displays the map view with all pending and approved obstacles
        /// </summary>
        /// <returns>The map view with obstacle data</returns>
        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var reportedObstacles = await _obstacleRepository.GetReportedObstacles();
            
            // Pass obstacles to view via ViewBag (required by Map.cshtml JavaScript serialization)
            ViewBag.ReportedObstacles = reportedObstacles;
            
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }

        /// <summary>
        /// Returns JSON data of all pending obstacles for map display
        /// </summary>
        /// <returns>JSON array of pending obstacles</returns>
        [HttpGet]
        public async Task<IActionResult> GetPendingObstacles()
        {
            var pendingObstacles = await _obstacleRepository.GetPendingObstacles();
            var pendingObstaclesJson = _obstacleRepository.MapToJsonFormat(pendingObstacles);
            
            return Json(pendingObstaclesJson);
        }
    }
}
