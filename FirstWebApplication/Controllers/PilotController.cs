using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for pilot functionality. Handles viewing and updating of obstacles owned by the logged-in pilot.
    /// </summary>
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

        /// <summary>
        /// Displays overview of all obstacles owned by the logged-in pilot
        /// </summary>
        /// <returns>The pilot's obstacles overview view, or Unauthorized if user is not logged in</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var myObstacles = await _obstacleRepository.GetObstaclesByOwner(user.Id);
            return View(myObstacles);
        }

        /// <summary>
        /// Displays details of a specific obstacle and its associated reports. Only obstacles owned by the logged-in pilot can be viewed.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to view</param>
        /// <returns>The obstacle details view, or redirects to index if obstacle not found or not owned by user</returns>
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

        /// <summary>
        /// Updates an obstacle. Only obstacles with status "Under treatment" (1) can be updated.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="obstacleName">The new name for the obstacle</param>
        /// <param name="obstacleDescription">The new description for the obstacle</param>
        /// <param name="obstacleHeight">The new height for the obstacle</param>
        /// <returns>Redirects to obstacle details page</returns>
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

            // Add a comment that the pilot updated the obstacle
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

