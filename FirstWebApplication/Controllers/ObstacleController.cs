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
            if (!ValidateQuickSave(obstacledata))
            {
                return PartialView("_ObstacleFormPartial", obstacledata);
            }
            
            return await SaveObstacleAndCreateReport(obstacledata, GenerateQuickSaveReportMessage);
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
                return PartialView("_ObstacleFormPartial", obstacledata);
            }
            
            return await SaveObstacleAndCreateReport(obstacledata, GenerateSubmitReportMessage);
        }

        /// <summary>
        /// Helper method to save obstacle, create report, and return appropriate response
        /// </summary>
        /// <param name="obstacleData">The obstacle data to save</param>
        /// <param name="generateReportMessage">Function to generate the report message based on the saved obstacle</param>
        /// <returns>JSON response if AJAX, otherwise returns overview view</returns>
        private async Task<IActionResult> SaveObstacleAndCreateReport(
            ObstacleData obstacleData,
            Func<ObstacleData, string> generateReportMessage)
        {
            // Set owner of the obstacle (logged-in pilot)
            await SetObstacleOwner(obstacleData);

            // Save the obstacle
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacleData);

            // Create automatic report when obstacle is created
            var rapport = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = generateReportMessage(savedObstacle)
            };

            await _registrarRepository.AddRapport(rapport);

            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, savedObstacle);
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
        /// <param name="savedObstacle">The saved obstacle data</param>
        /// <returns>JSON response if AJAX, otherwise returns overview view</returns>
        private IActionResult ReturnJsonOrViewIfAjax(int obstacleId, ObstacleData savedObstacle)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = obstacleId }) });
            }
            return View("Overview", savedObstacle);
        }

        /// <summary>
        /// Validates obstacle data for quick save (only geometry is required)
        /// </summary>
        /// <param name="obstacleData">The obstacle data to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateQuickSave(ObstacleData obstacleData)
        {
            // Set default values for fields that can be skipped (database doesn't allow null)
            obstacleData.ObstacleName = obstacleData.ObstacleName ?? string.Empty;
            obstacleData.ObstacleDescription = obstacleData.ObstacleDescription ?? string.Empty;

            // Remove validation errors for fields that can be skipped
            var fieldsToSkip = new[] 
            { 
                nameof(ObstacleData.ObstacleName), 
                nameof(ObstacleData.ObstacleHeight), 
                nameof(ObstacleData.ObstacleDescription) 
            };
            
            foreach (var field in fieldsToSkip)
            {
                ModelState.Remove(field);
            }

            // Validate only GeometryGeoJson (required field)
            if (string.IsNullOrEmpty(obstacleData.GeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleData.GeometryGeoJson), "Geometry (GeoJSON) is required.");
                return false;
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// Generates report message for quick save operation
        /// </summary>
        /// <param name="obstacle">The saved obstacle</param>
        /// <returns>Formatted report message</returns>
        private string GenerateQuickSaveReportMessage(ObstacleData obstacle)
        {
            string obstacleName;
            if (string.IsNullOrEmpty(obstacle.ObstacleName))
                obstacleName = "Ikke navngitt";
            else
                obstacleName = obstacle.ObstacleName;

            string heightInfo;
            if (obstacle.ObstacleHeight > 0)
                heightInfo = $"Høyde: {obstacle.ObstacleHeight}m. ";
            else
                heightInfo = "";

            string descriptionInfo;
            if (!string.IsNullOrEmpty(obstacle.ObstacleDescription))
                descriptionInfo = obstacle.ObstacleDescription;
            else
                descriptionInfo = "";

            return $"Hindring '{obstacleName}' ble hurtiglagret. {heightInfo}{descriptionInfo}";
        }

        /// <summary>
        /// Generates report message for full submit operation
        /// </summary>
        /// <param name="obstacle">The saved obstacle</param>
        /// <returns>Formatted report message</returns>
        private string GenerateSubmitReportMessage(ObstacleData obstacle)
        {
            return $"Hindring '{obstacle.ObstacleName}' ble sendt inn. Høyde: {obstacle.ObstacleHeight}m. {obstacle.ObstacleDescription}";
        }
    }
}
