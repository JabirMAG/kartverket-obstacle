using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;

namespace FirstWebApplication.Controllers
{
    // Controller for kartfunksjonalitet. Håndterer visning av hindringer på kartet.
    [Authorize(Roles = "Pilot")]
    public class MapController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public MapController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        // Viser kartvisningen med alle hindringer under behandling og godkjente hindringer
        [HttpGet]
        public async Task<IActionResult> Map()
        {
            var reportedObstacles = await _obstacleRepository.GetReportedObstacles();
            
            // Send hindringer til view via ViewBag (kreves av Map.cshtml JavaScript-serialisering)
            ViewBag.ReportedObstacles = reportedObstacles;
            
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }

        // Returnerer JSON-data for alle hindringer under behandling for kartvisning
        [HttpGet]
        public async Task<IActionResult> GetPendingObstacles()
        {
            var pendingObstacles = await _obstacleRepository.GetPendingObstacles();
            var pendingObstaclesJson = _obstacleRepository.MapToJsonFormat(pendingObstacles);
            
            return Json(pendingObstaclesJson);
        }
    }
}
