using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRepository _userRepository;

        // Error messages
        private const string ErrorObstacleNotFound = "Fant ikke hindring.";
        private const string ErrorCannotEditObstacle = "Du kan kun redigere mens hindringen er under behandling.";
        private const string SuccessObstacleUpdated = "Hindringen er oppdatert.";

        public PilotController(
            IObstacleRepository obstacleRepository,
            IRegistrarRepository registrarRepository,
            IUserRepository userRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Displays overview of all obstacles owned by the logged-in pilot
        /// </summary>
        /// <returns>The pilot's obstacles overview view, or Unauthorized if user is not logged in</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userRepository.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

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
            var user = await _userRepository.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var obstacle = await _obstacleRepository.GetObstacleByOwnerAndId(obstacleId, user.Id);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Index));
            }

            var obstacleRapports = await _registrarRepository.GetRapportsByObstacleId(obstacleId);

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View("DetaljerOmRapport", obstacle);
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
            var user = await _userRepository.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var obstacle = await _obstacleRepository.GetObstacleByOwnerAndId(obstacleId, user.Id);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Index));
            }

            var canEdit = await _obstacleRepository.CanEditObstacle(obstacleId);
            if (!canEdit)
            {
                TempData["Error"] = ErrorCannotEditObstacle;
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            var updatedObstacle = await _obstacleRepository.UpdateObstacleProperties(obstacleId, obstacleName, obstacleDescription, obstacleHeight);
            if (updatedObstacle != null)
            {
                var reportMessage = _registrarRepository.GenerateUpdateReportMessage(updatedObstacle, obstacleHeight);
                await _registrarRepository.AddRapportToObstacle(updatedObstacle.ObstacleId, reportMessage);
            }

            TempData["Success"] = SuccessObstacleUpdated;
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }
    }
}

