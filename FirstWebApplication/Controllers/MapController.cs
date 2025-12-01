using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for map functionality. Handles display of obstacles on the map.
    /// </summary>
    public class MapController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        // Obstacle status constants
        private const int StatusPending = 1;      // Under behandling
        private const int StatusApproved = 2;     // Godkjent
        private const int StatusRejected = 3;     // Avslått

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
            var reportedObstacles = await GetReportedObstaclesAsync();
            
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
            var pendingObstacles = await GetPendingObstaclesAsync();
            var pendingObstaclesJson = MapToJsonFormat(pendingObstacles);
            
            return Json(pendingObstaclesJson);
        }

        /// <summary>
        /// Gets all obstacles from the repository (cached for reuse within same request)
        /// </summary>
        /// <returns>List of all obstacles</returns>
        private async Task<List<ObstacleData>> GetAllObstaclesAsync()
        {
            var obstacles = await _obstacleRepository.GetAllObstacles();
            return obstacles.ToList();
        }

        /// <summary>
        /// Gets all obstacles that are pending or approved (status 1 or 2)
        /// </summary>
        /// <returns>List of reported obstacles</returns>
        private async Task<List<ObstacleData>> GetReportedObstaclesAsync()
        {
            var allObstacles = await GetAllObstaclesAsync();
            return allObstacles
                .Where(o => o.ObstacleStatus == StatusPending || o.ObstacleStatus == StatusApproved)
                .ToList();
        }

        /// <summary>
        /// Gets all pending obstacles (status 1)
        /// </summary>
        /// <returns>List of pending obstacles</returns>
        private async Task<List<ObstacleData>> GetPendingObstaclesAsync()
        {
            var allObstacles = await GetAllObstaclesAsync();
            return allObstacles
                .Where(o => o.ObstacleStatus == StatusPending)
                .ToList();
        }

        /// <summary>
        /// Maps ObstacleData to JSON-friendly format with lowercase property names
        /// </summary>
        /// <param name="obstacles">List of obstacles to map</param>
        /// <returns>List of anonymous objects in JSON format</returns>
        private static List<object> MapToJsonFormat(IEnumerable<ObstacleData> obstacles)
        {
            return obstacles
                .Select(o => new
                {
                    id = o.ObstacleId,
                    name = o.ObstacleName,
                    height = o.ObstacleHeight,
                    description = o.ObstacleDescription,
                    status = o.ObstacleStatus,
                    geometryGeoJson = o.GeometryGeoJson
                })
                .ToList<object>();
        }
    }
}
