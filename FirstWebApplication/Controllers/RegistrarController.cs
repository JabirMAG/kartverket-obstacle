using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for registrar functionality. Handles processing of obstacles and reports.
    /// </summary>
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

        /// <summary>
        /// Displays main overview for registrar with all obstacles and reports
        /// </summary>
        /// <returns>The registrar overview view</returns>
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

        /// <summary>
        /// Updates the status of an obstacle. If status is Rejected (3), the obstacle is archived.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="status">The new status for the obstacle</param>
        /// <param name="returnUrl">Optional return URL to redirect back to details page</param>
        /// <returns>Redirects to registrar page or details page based on returnUrl</returns>
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

            // If status is Rejected, archive the obstacle and all reports
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

            // Redirect back to details page if returnUrl is set, otherwise to reports
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
                // If status is Rejected, redirect to reports since the obstacle is deleted
                if (status == StatusRejected)
                {
                    return RedirectToAction(nameof(Registrar));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            return RedirectToAction(nameof(Registrar));
        }

        /// <summary>
        /// Adds a new report/comment to an obstacle. Comment is optional - if empty, method returns without error.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="rapportComment">The comment text to add (optional)</param>
        /// <returns>Redirects to registrar page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRapport(int obstacleId, string rapportComment)
        {
            // If comment is empty, silently return without error
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

        /// <summary>
        /// Displays detailed view of an obstacle and all its associated reports
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to view</param>
        /// <returns>The obstacle details view, or redirects to registrar page if obstacle not found</returns>
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

            return View("DetaljerOmRapport", obstacle);
        }

        /// <summary>
        /// Adds a comment to an obstacle's report
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="comment">The comment text to add</param>
        /// <returns>Redirects to obstacle details page</returns>
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

        /// <summary>
        /// Displays all archived reports (rejected obstacles)
        /// </summary>
        /// <returns>The archived reports view</returns>
        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _archiveRepository.GetAllArchivedReportsAsync();
            return View("RegistrarArchivedReports", archivedReports);
        }
    }
}