using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using FirstWebApplication.Repositories;

namespace FirstWebApplication.Controllers
{
    public class RegistrarController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;

        public RegistrarController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
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
            await _obstacleRepository.UpdateObstacles(obstacle);
            TempData["Success"] = $"Status for hindring '{obstacle.ObstacleName}' er oppdatert.";

            // Redirect tilbake til detaljsiden hvis returnUrl er satt, ellers til rapporter
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
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
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Registrar", obstacledata);
        }
    }
}