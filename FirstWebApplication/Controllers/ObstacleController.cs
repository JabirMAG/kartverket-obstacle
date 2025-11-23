using FirstWebApplication.DataContext;
using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for obstacle management. Handles creation, viewing, and submission of obstacles.
    /// </summary>
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

        /// <summary>
        /// Returns the obstacle form as a partial view
        /// </summary>
        /// <returns>The obstacle form partial view</returns>
        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleData());
        }

        /// <summary>
        /// Displays overview of a specific obstacle
        /// </summary>
        /// <param name="id">The ID of the obstacle to view</param>
        /// <returns>The obstacle overview view, or NotFound if obstacle doesn't exist</returns>
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

        /// <summary>
        /// Quick saves an obstacle with minimal required data (only geometry is required). Automatically creates a report entry when obstacle is saved.
        /// </summary>
        /// <param name="obstacledata">The obstacle data to save</param>
        /// <returns>JSON response with redirect URL if AJAX, otherwise returns overview view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickSaveObstacle(ObstacleData obstacledata)
        {
            // Set default values first for fields that can be skipped (database doesn't allow null)
            // This must be done explicitly because empty form fields can come in as null
            obstacledata.ObstacleName = obstacledata.ObstacleName ?? string.Empty;
            obstacledata.ObstacleDescription = obstacledata.ObstacleDescription ?? string.Empty;
            // ObstacleHeight is already 0 as default for double, so it doesn't need to be set

            // Remove validation errors for the three fields that can be skipped
            ModelState.Remove(nameof(ObstacleData.ObstacleName));
            ModelState.Remove(nameof(ObstacleData.ObstacleHeight));
            ModelState.Remove(nameof(ObstacleData.ObstacleDescription));

            // Validate only GeometryGeoJson (required field)
            if (string.IsNullOrEmpty(obstacledata.GeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleData.GeometryGeoJson), "Geometry (GeoJSON) is required.");
            }

            if (!ModelState.IsValid)
            {
                return ReturnPartialViewIfAjax("_ObstacleFormPartial", obstacledata);
            }
            
            // Set owner of the obstacle (logged-in pilot)
            await SetObstacleOwner(obstacledata);

            // Save the obstacle (with empty values for the three fields)
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacledata);
            
            // Create automatic report when obstacle is created
            var obstacleName = string.IsNullOrEmpty(savedObstacle.ObstacleName) ? "Ikke navngitt" : savedObstacle.ObstacleName;
            var heightInfo = savedObstacle.ObstacleHeight > 0 ? $"Høyde: {savedObstacle.ObstacleHeight}m. " : "";
            var descriptionInfo = !string.IsNullOrEmpty(savedObstacle.ObstacleDescription) ? savedObstacle.ObstacleDescription : "";
            
            var rapport = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = $"Hindring '{obstacleName}' ble hurtiglagret. {heightInfo}{descriptionInfo}"
            };
            
            await _registrarRepository.AddRapport(rapport);
            
            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, obstacledata);
        }

        /// <summary>
        /// Submits a fully completed obstacle with all required fields validated. Automatically creates a report entry when obstacle is saved.
        /// </summary>
        /// <param name="obstacledata">The complete obstacle data to submit</param>
        /// <returns>JSON response with redirect URL if AJAX, otherwise returns overview view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return ReturnPartialViewIfAjax("_ObstacleFormPartial", obstacledata);
            }
            
            // Set owner of the obstacle (logged-in pilot)
            await SetObstacleOwner(obstacledata);

            // Save the obstacle
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacledata);
            
            // Create automatic report when obstacle is created
            var rapport = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = $"Hindring '{savedObstacle.ObstacleName}' ble sendt inn. Høyde: {savedObstacle.ObstacleHeight}m. {savedObstacle.ObstacleDescription}"
            };
            
            await _registrarRepository.AddRapport(rapport);
            
            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, obstacledata);
        }

        /// <summary>
        /// Helper method to check if request is AJAX
        /// </summary>
        /// <returns>True if the request is an AJAX request, false otherwise</returns>
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
        }

        /// <summary>
        /// Helper method to return partial view if AJAX, otherwise regular view
        /// </summary>
        /// <param name="viewName">The name of the partial view to return</param>
        /// <param name="model">The model to pass to the view</param>
        /// <returns>The partial view</returns>
        private IActionResult ReturnPartialViewIfAjax(string viewName, object model)
        {
            return PartialView(viewName, model);
        }

        /// <summary>
        /// Helper method to set obstacle owner from current user
        /// </summary>
        /// <param name="obstacle">The obstacle to set the owner for</param>
        private async Task SetObstacleOwner(ObstacleData obstacle)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                obstacle.OwnerUserId = currentUser.Id;
            }
        }

        /// <summary>
        /// Helper method to return JSON if AJAX request, otherwise return view
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="obstacle">The obstacle data</param>
        /// <returns>JSON response if AJAX, otherwise returns overview view</returns>
        private IActionResult ReturnJsonOrViewIfAjax(int obstacleId, ObstacleData obstacle)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = obstacleId }) });
            }
            return View("Overview", obstacle);
        }
    }
}
