using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using FirstWebApplication.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        private readonly ApplicationDBContext _context;
        private readonly IArchiveRepository _archiveRepository;

        public RegistrarController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository, ApplicationDBContext context, IArchiveRepository archiveRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _context = context;
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
        /// Updates the status of an obstacle. If status is 3 (Rejected), the obstacle is archived.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="status">The new status for the obstacle</param>
        /// <param name="returnUrl">Optional return URL to redirect back to details page</param>
        /// <returns>Redirects to registrar page or details page based on returnUrl</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleStatus(int obstacleId, int status, string returnUrl = null)
        {
            var obstacle = await GetObstacleOrRedirect(obstacleId);
            if (obstacle == null)
            {
                return RedirectToAction(nameof(Registrar));
            }

            obstacle.ObstacleStatus = status;
            
            // If status is Rejected (3), archive the obstacle and all reports
            if (status == 3)
            {
                var archivedReportCount = await _archiveRepository.ArchiveObstacleAsync(obstacle);
                TempData["Success"] = $"Hindring '{obstacle.ObstacleName}' er avvist og arkivert sammen med {archivedReportCount} rapport(er).";
            }
            else
            {
                await _obstacleRepository.UpdateObstacles(obstacle);
                TempData["Success"] = $"Status for hindring '{obstacle.ObstacleName}' er oppdatert.";
            }

            // Redirect back to details page if returnUrl is set, otherwise to reports
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
                // If status is Rejected, redirect to reports since the obstacle is deleted
                if (status == 3)
                {
                    return RedirectToAction(nameof(Registrar));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            return RedirectToAction(nameof(Registrar));
        }

        /// <summary>
        /// Adds a new report/comment to an obstacle
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="rapportComment">The comment text to add</param>
        /// <returns>Redirects to registrar page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRapport(int obstacleId, string rapportComment)
        {
            if (string.IsNullOrWhiteSpace(rapportComment))
            {
                return RedirectToAction(nameof(Registrar));
            }

            await AddRapportToObstacle(obstacleId, rapportComment);
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
            var obstacle = await GetObstacleOrRedirect(obstacleId);
            if (obstacle == null)
            {
                return RedirectToAction(nameof(Registrar));
            }

            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();

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
                TempData["Error"] = "Kommentar kan ikke være tom.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            var obstacle = await GetObstacleOrRedirect(obstacleId);
            if (obstacle == null)
            {
                return RedirectToAction(nameof(Registrar));
            }

            await AddRapportToObstacle(obstacleId, comment);
            TempData["Success"] = "Kommentar lagt til.";
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }

        /// <summary>
        /// Displays all archived reports (rejected obstacles)
        /// </summary>
        /// <returns>The archived reports view</returns>
        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
            
            return View("RegistrarArchivedReports", archivedReports);
        }

        /// <summary>
        /// Helper method to get obstacle by ID or redirect with error if not found
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to get</param>
        /// <returns>The obstacle if found, null if not found (after redirecting)</returns>
        private async Task<ObstacleData?> GetObstacleOrRedirect(int obstacleId)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return null;
            }
            return obstacle;
        }

        /// <summary>
        /// Helper method to add a report/comment to an obstacle
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="comment">The comment text to add</param>
        private async Task AddRapportToObstacle(int obstacleId, string comment)
        {
            var rapport = new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = comment
            };

            await _registrarRepository.AddRapport(rapport);
        }
    }
}