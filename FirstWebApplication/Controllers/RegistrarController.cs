using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // Controller for registerfører-funksjonalitet. Håndterer behandling av hindringer og rapporter.
    [Authorize(Roles = "Admin,Registerfører")]
    public class RegistrarController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IArchiveRepository _archiveRepository;

        // Obstacle status constants
        private const int StatusRejected = 3;     // Avslått

        // Error messages
        private const string ErrorObstacleNotFound = "Fant ikke hindring.";
        private const string ErrorCommentEmpty = "Kommentar kan ikke være tom.";

        // Success messages
        private const string SuccessCommentAdded = "Kommentar lagt til.";

        public RegistrarController(
            IObstacleRepository obstacleRepository, 
            IRegistrarRepository registrarRepository, 
            IArchiveRepository archiveRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _archiveRepository = archiveRepository;
        }

        // Viser hovedoversikt for registerfører med alle hindringer og rapporter
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var obstacles = await _obstacleRepository.GetAllObstacles();
            var rapports = await _registrarRepository.GetAllRapports();

            var vm = new RegistrarViewModel
            {
                Obstacles = obstacles,
                Rapports = rapports
            };

            return View(vm);
        }

        // Oppdaterer statusen til en hindring. Hvis status er Avslått (3), arkiveres hindringen.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleStatus(int obstacleId, int status, string returnUrl = null)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Registrar));
            }

            // Hvis status er Avslått, arkiver hindringen og alle rapporter
            if (status == StatusRejected)
            {
                var archivedReportCount = await _archiveRepository.ArchiveObstacleAsync(obstacle);
                TempData["Success"] = $"Hindring '{obstacle.ObstacleName}' er avvist og arkivert sammen med {archivedReportCount} rapport(er).";
            }
            else
            {
                var updatedObstacle = await _obstacleRepository.UpdateObstacleStatus(obstacleId, status);
                if (updatedObstacle != null)
                {
                    TempData["Success"] = $"Status for hindring '{updatedObstacle.ObstacleName}' er oppdatert.";
                }
            }

            // Omdiriger tilbake til detaljsiden hvis returnUrl er satt, ellers til rapporter
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("RegistrarObstacleDetails"))
            {
                // Hvis status er Avslått, omdiriger til rapporter siden hindringen er slettet
                if (status == StatusRejected)
                {
                    return RedirectToAction(nameof(Registrar));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            return RedirectToAction(nameof(Registrar));
        }

        // Legger til en ny rapport/kommentar til en hindring. Kommentar er valgfri - hvis tom, returnerer metoden uten feil.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRapport(int obstacleId, string rapportComment)
        {
            // Hvis kommentar er tom, returner stille uten feil
            if (string.IsNullOrWhiteSpace(rapportComment))
            {
                return RedirectToAction(nameof(Registrar));
            }

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Registrar));
            }

            await _registrarRepository.AddRapportToObstacle(obstacleId, rapportComment);
            TempData["Success"] = SuccessCommentAdded;
            return RedirectToAction(nameof(Registrar));
        }

        // Viser detaljert visning av en hindring og alle tilknyttede rapporter
        [HttpGet]
        public async Task<IActionResult> DetaljerOmRapport(int obstacleId)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Registrar));
            }

            var obstacleRapports = await _registrarRepository.GetRapportsByObstacleId(obstacleId);

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View("RegistrarObstacleDetails", obstacle);
        }

        // Legger til en kommentar til en hindringsrapport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int obstacleId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = ErrorCommentEmpty;
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Registrar));
            }

            await _registrarRepository.AddRapportToObstacle(obstacleId, comment);
            TempData["Success"] = SuccessCommentAdded;
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }

        // Viser alle arkiverte rapporter (avviste hindringer)
        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _archiveRepository.GetAllArchivedReportsAsync();
            return View("RegistrarArchivedReports", archivedReports);
        }
    }
}