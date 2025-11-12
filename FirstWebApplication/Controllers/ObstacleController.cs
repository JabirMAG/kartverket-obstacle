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
        public async Task<IActionResult> QuickSaveObstacle(ObstacleData obstacledata)
        {
            // Sett standardverdier FØRST for feltene som kan hoppes over (databasen tillater ikke null)
            // Dette må gjøres eksplisitt fordi tomme form-felt kan komme inn som null
            obstacledata.ObstacleName = obstacledata.ObstacleName ?? string.Empty;
            obstacledata.ObstacleDescription = obstacledata.ObstacleDescription ?? string.Empty;
            // ObstacleHeight er allerede 0 som default for double, så den trenger ikke settes

            // Fjern valideringsfeil for de tre feltene som kan hoppes over
            ModelState.Remove(nameof(ObstacleData.ObstacleName));
            ModelState.Remove(nameof(ObstacleData.ObstacleHeight));
            ModelState.Remove(nameof(ObstacleData.ObstacleDescription));

            // Valider kun GeometryGeoJson (påkrevd felt)
            if (string.IsNullOrEmpty(obstacledata.GeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleData.GeometryGeoJson), "Geometry (GeoJSON) is required.");
            }

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

            // Lagre hindringen (med tomme verdier for de tre feltene)
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacledata);
            
            // Opprett automatisk rapport når hindring opprettes
            var obstacleName = string.IsNullOrEmpty(savedObstacle.ObstacleName) ? "Ikke navngitt" : savedObstacle.ObstacleName;
            var heightInfo = savedObstacle.ObstacleHeight > 0 ? $"Høyde: {savedObstacle.ObstacleHeight}m. " : "";
            var descriptionInfo = !string.IsNullOrEmpty(savedObstacle.ObstacleDescription) ? savedObstacle.ObstacleDescription : "";
            
            var rapport = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = $"Hindring '{obstacleName}' ble hurtiglagret. {heightInfo}{descriptionInfo}",
                ReportedByUserId = currentUser?.Id
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
                RapportComment = $"Hindring '{savedObstacle.ObstacleName}' ble sendt inn. Høyde: {savedObstacle.ObstacleHeight}m. {savedObstacle.ObstacleDescription}",
                ReportedByUserId = currentUser?.Id
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
