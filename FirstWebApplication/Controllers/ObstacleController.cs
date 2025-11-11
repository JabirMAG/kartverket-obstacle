using FirstWebApplication.DataContext;
using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ObstacleController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository, UserManager<ApplicationUser> userManager)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userManager = userManager;
        }

        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleData());
        }

        [HttpGet]
        public async Task<IActionResult> Overview(int id)
        {
            var obstacle = await _obstacleRepository.GetElementById(id);
            if (obstacle == null)
            {
                return NotFound();
            }
            return View(obstacle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                // Check if this is an AJAX request
                var request = Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
                if (request)
                {
                    return PartialView("_ObstacleFormPartial", obstacledata);
                }
                return PartialView("_ObstacleFormPartial", obstacledata);
            }
            
            // Sett eier av hindringen (innlogget pilot)
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                obstacledata.OwnerUserId = currentUser.Id;
            }

            // Lagre hindringen
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacledata);
            
            // Opprett automatisk rapport når hindring opprettes
            var rapport = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = $"Hindring '{savedObstacle.ObstacleName}' ble sendt inn. Høyde: {savedObstacle.ObstacleHeight}m. {savedObstacle.ObstacleDescription}"
            };
            
            await _registrarRepository.AddRapport(rapport);
            
            // Check if this is an AJAX request
            var isAjaxRequest = Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
            if (isAjaxRequest)
            {
                // For AJAX requests, return a JSON response with redirect URL
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = savedObstacle.ObstacleId }) });
            }
            
            return View("Overview", obstacledata);
        }
    }
}
