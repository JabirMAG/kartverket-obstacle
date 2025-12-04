using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    // Controller for pilot-funksjonalitet. Håndterer visning og oppdatering av hindringer eid av innlogget pilot.
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

        // Viser oversikt over alle hindringer eid av innlogget pilot
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

        // Viser detaljer om en spesifikk hindring og dens tilknyttede rapporter. Kun hindringer eid av innlogget pilot kan vises.
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

            return View("PilotObstacleDetails", obstacle);
        }

        // Oppdaterer en hindring. Kun hindringer med status "Under behandling" (1) kan oppdateres.
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

