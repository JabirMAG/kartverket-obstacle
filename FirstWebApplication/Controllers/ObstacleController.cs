using FirstWebApplication.DataContext;
using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public ObstacleController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleData());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ObstacleFormPartial", obstacledata);
            }
            await _obstacleRepository.AddObstacle(obstacledata);
            return View("Overview", obstacledata);
        }
        public IActionResult Overview(int id)
        {
            var obstacle = _obstacleRepository.GetElementById(id).Result;
            if (obstacle == null)
                return NotFound();

            return View(obstacle);
        }
    }
}
