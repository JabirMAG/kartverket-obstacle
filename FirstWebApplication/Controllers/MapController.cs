using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;

namespace FirstWebApplication.Controllers
{
    // Controller for kart-funksjonalitet. Håndterer visning av hindringer på kartet.
    [Authorize(Roles = "Pilot")]
    public class MapController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public MapController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        // Viser kartvisningen med alle ventende og godkjente hindringer
        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var reportedObstacles = await _obstacleRepository.GetReportedObstacles();
            
            // Sender hindringer til visning via ViewBag (påkrevd av Map.cshtml JavaScript serialisering)
            ViewBag.ReportedObstacles = reportedObstacles;
            
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }

        // Returnerer JSON-data for alle ventende hindringer for kartvisning
        [HttpGet]
        public async Task<IActionResult> GetPendingObstacles()
        {
            var pendingObstacles = await _obstacleRepository.GetPendingObstacles();
            var pendingObstaclesJson = _obstacleRepository.MapToJsonFormat(pendingObstacles);
            
            return Json(pendingObstaclesJson);
        }
    }
}
