using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using FirstWebApplication.Services;
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
        private readonly IPilotHelperService _pilotHelperService;

        // Obstacle status constants
        private const int StatusPending = 1;      // Under behandling

        // Error messages
        private const string ErrorObstacleNotFound = "Fant ikke hindring.";
        private const string ErrorCannotEditObstacle = "Du kan kun redigere mens hindringen er under behandling.";
        private const string SuccessObstacleUpdated = "Hindringen er oppdatert.";

        public PilotController(
            IObstacleRepository obstacleRepository,
            IRegistrarRepository registrarRepository,
            UserManager<ApplicationUser> userManager,
            IPilotHelperService pilotHelperService)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userManager = userManager;
            _pilotHelperService = pilotHelperService;
        }

        /// <summary>
        /// Displays overview of all obstacles owned by the logged-in pilot
        /// </summary>
        /// <returns>The pilot's obstacles overview view, or Unauthorized if user is not logged in</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (user, userError) = await GetUserOrErrorAsync();
            if (userError != null) return userError;

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
            var (user, userError) = await GetUserOrErrorAsync();
            if (userError != null) return userError;

            var obstacle = await _pilotHelperService.GetUserObstacleAsync(obstacleId, user.Id);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Index));
            }

            var obstacleRapports = await _pilotHelperService.GetObstacleRapportsAsync(obstacleId);

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
            var (user, userError) = await GetUserOrErrorAsync();
            if (userError != null) return userError;

            var obstacle = await _pilotHelperService.GetUserObstacleAsync(obstacleId, user.Id);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Index));
            }

            if (!CanEditObstacle(obstacle))
            {
                TempData["Error"] = ErrorCannotEditObstacle;
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            UpdateObstacleProperties(obstacle, obstacleName, obstacleDescription, obstacleHeight);
            await _obstacleRepository.UpdateObstacles(obstacle);
            await CreateUpdateRapportAsync(obstacle.ObstacleId, obstacleHeight);

            TempData["Success"] = SuccessObstacleUpdated;
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }

        /// <summary>
        /// Gets the current logged-in user or returns an error result if user is not found
        /// </summary>
        /// <returns>Tuple containing the user (or null) and error result (or null if user is valid)</returns>
        private async Task<(ApplicationUser? user, IActionResult? errorResult)> GetUserOrErrorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return (null, Unauthorized());
            }
            return (user, null);
        }


        /// <summary>
        /// Checks if an obstacle can be edited (must have pending status)
        /// </summary>
        /// <param name="obstacle">The obstacle to check</param>
        /// <returns>True if obstacle can be edited, otherwise false</returns>
        private static bool CanEditObstacle(ObstacleData obstacle)
        {
            return obstacle.ObstacleStatus == StatusPending;
        }

        /// <summary>
        /// Updates the properties of an obstacle
        /// </summary>
        /// <param name="obstacle">The obstacle to update</param>
        /// <param name="name">The new name</param>
        /// <param name="description">The new description</param>
        /// <param name="height">The new height</param>
        private static void UpdateObstacleProperties(ObstacleData obstacle, string name, string description, double height)
        {
            obstacle.ObstacleName = name;
            obstacle.ObstacleDescription = description;
            obstacle.ObstacleHeight = height;
        }

        /// <summary>
        /// Creates a rapport comment indicating that the pilot updated the obstacle
        /// </summary>
        /// <param name="obstacleId">The ID of the updated obstacle</param>
        /// <param name="newHeight">The new height of the obstacle</param>
        private async Task CreateUpdateRapportAsync(int obstacleId, double newHeight)
        {
            var rapport = new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = $"Piloten oppdaterte hindringen. Ny høyde: {newHeight}m."
            };
            await _registrarRepository.AddRapport(rapport);
        }
    }
}

