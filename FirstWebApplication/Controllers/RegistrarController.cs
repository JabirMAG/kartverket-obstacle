using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using FirstWebApplication.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Admin,Registerfører")]
    public class RegistrarController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly ApplicationDBContext _context;

        public RegistrarController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository, ApplicationDBContext context)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _context = context;
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleStatus(int obstacleId, int status, string returnUrl = null)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Registrar));
            }

            obstacle.ObstacleStatus = status;
            
            // Hvis status er Rejected (3), arkiver hindringen og alle rapporter
            if (status == 3)
            {
                // Hent alle rapporter knyttet til hindringen
                var rapports = await _context.Rapports
                    .Where(r => r.ObstacleId == obstacle.ObstacleId)
                    .ToListAsync();
                
                // Kombiner alle rapport-kommentarer til en JSON array
                var rapportComments = rapports.Select(r => r.RapportComment).ToList();
                var rapportCommentsJson = JsonSerializer.Serialize(rapportComments);
                
                var archivedReport = new ArchivedReport
                {
                    OriginalObstacleId = obstacle.ObstacleId,
                    ObstacleName = obstacle.ObstacleName,
                    ObstacleHeight = obstacle.ObstacleHeight,
                    ObstacleDescription = obstacle.ObstacleDescription,
                    GeometryGeoJson = obstacle.GeometryGeoJson,
                    ObstacleStatus = 3,
                    ArchivedDate = DateTime.UtcNow,
                    RapportComments = rapportCommentsJson
                };
                
                await _context.ArchivedReports.AddAsync(archivedReport);
                
                // Slett rapporter fra Rapports-tabellen (de er nå lagret i ArchivedReport)
                if (rapports.Any())
                {
                    _context.Rapports.RemoveRange(rapports);
                }
                
                await _context.SaveChangesAsync();
                
                // Slett hindringen fra ObstaclesData (som UpdateObstacles gjør når status er 3)
                await _obstacleRepository.UpdateObstacles(obstacle);
                
                TempData["Success"] = $"Hindring '{obstacle.ObstacleName}' er avvist og arkivert sammen med {rapports.Count} rapport(er).";
            }
            else
            {
                await _obstacleRepository.UpdateObstacles(obstacle);
                TempData["Success"] = $"Status for hindring '{obstacle.ObstacleName}' er oppdatert.";
            }

            // Redirect tilbake til detaljsiden hvis returnUrl er satt, ellers til rapporter
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
                // Hvis status er Rejected, redirect til rapporter siden hindringen er slettet
                if (status == 3)
                {
                    return RedirectToAction(nameof(Registrar));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            return RedirectToAction(nameof(Registrar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRapport(int obstacleId, string rapportComment)
        {
            if (string.IsNullOrWhiteSpace(rapportComment))
                return RedirectToAction(nameof(Registrar));

            var rapport = new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = rapportComment
            };

            await _registrarRepository.AddRapport(rapport);
            return RedirectToAction(nameof(Registrar));
        }

        [HttpGet]
        public async Task<IActionResult> DetaljerOmRapport(int obstacleId)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Registrar));
            }

            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View("DetaljerOmRapport", obstacle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int obstacleId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Kommentar kan ikke være tom.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Registrar));
            }

            var rapport = new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = comment
            };

            await _registrarRepository.AddRapport(rapport);
            TempData["Success"] = "Kommentar lagt til.";
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Registrar", obstacledata);
        }

        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
            
            return View("RegistrarArchivedReports", archivedReports);
        }
    }
}