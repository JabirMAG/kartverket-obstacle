using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PilotController(
            IObstacleRepository obstacleRepository,
            IRegistrarRepository registrarRepository,
            UserManager<ApplicationUser> userManager)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var myObstacles = await _obstacleRepository.GetObstaclesByOwner(user.Id);
            return View(myObstacles);
        }

        [HttpGet]
        public async Task<IActionResult> DetaljerOmRapport(int obstacleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null || obstacle.OwnerUserId != user.Id)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }

            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View(obstacle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacle(int obstacleId, string obstacleName, string obstacleDescription, double obstacleHeight)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null || obstacle.OwnerUserId != user.Id)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }

            if (obstacle.ObstacleStatus != 1)
            {
                TempData["Error"] = "Du kan kun redigere mens hindringen er under behandling.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            obstacle.ObstacleName = obstacleName;
            obstacle.ObstacleDescription = obstacleDescription;
            obstacle.ObstacleHeight = obstacleHeight;

            await _obstacleRepository.UpdateObstacles(obstacle);

            // Legg igjen en kommentar om at piloten oppdaterte
            await _registrarRepository.AddRapport(new RapportData
            {
                ObstacleId = obstacle.ObstacleId,
                RapportComment = $"Piloten oppdaterte hindringen. Ny høyde: {obstacle.ObstacleHeight}m."
            });

            TempData["Success"] = "Hindringen er oppdatert.";
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }
    }
}

