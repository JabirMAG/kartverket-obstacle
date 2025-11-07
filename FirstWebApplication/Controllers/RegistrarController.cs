using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> UpdateObstacleStatus(int obstacleId, int status)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
                return NotFound();

            obstacle.ObstacleStatus = status;
            await _obstacleRepository.UpdateObstacles(obstacle);

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

        [HttpPost]
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Registrar", obstacledata);
        }
    }
}