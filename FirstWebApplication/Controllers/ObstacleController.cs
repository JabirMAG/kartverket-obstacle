using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for obstacle management. Handles creation, viewing, and submission of obstacles.
    /// </summary>
    public class ObstacleController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IUserRepository _userRepository;

        public ObstacleController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository, IUserRepository userRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Returns the obstacle form as a partial view
        /// </summary>
        /// <returns>The obstacle form partial view</returns>
        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleDataViewModel());
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
            var viewModel = _obstacleRepository.MapToViewModel(obstacle);
            return View(viewModel);
        }

        /// <summary>
        /// Quick saves an obstacle with minimal required data (only geometry is required). Automatically creates a report entry when obstacle is saved.
        /// </summary>
        /// <param name="viewModel">The obstacle ViewModel to save</param>
        /// <returns>JSON response with redirect URL if AJAX, otherwise returns overview view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickSaveObstacle(ObstacleDataViewModel viewModel)
        {
            if (!ValidateQuickSave(viewModel))
            {
                return PartialView("_ObstacleFormPartial", viewModel);
            }
            
            return await SaveObstacleAndCreateReport(viewModel, _registrarRepository.GenerateQuickSaveReportMessage);
        }

        /// <summary>
        /// Submits a fully completed obstacle with all required fields validated. Automatically creates a report entry when obstacle is saved.
        /// </summary>
        /// <param name="viewModel">The complete obstacle ViewModel to submit</param>
        /// <returns>JSON response with redirect URL if AJAX, otherwise returns overview view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitObstacle(ObstacleDataViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ObstacleFormPartial", viewModel);
            }
            
            return await SaveObstacleAndCreateReport(viewModel, _registrarRepository.GenerateSubmitReportMessage);
        }

        /// <summary>
        /// Helper method to save obstacle, create report, and return appropriate response
        /// </summary>
        /// <param name="viewModel">The obstacle ViewModel to save</param>
        /// <param name="generateReportMessage">Function to generate the report message based on the saved obstacle</param>
        /// <returns>JSON response if AJAX, otherwise returns overview view</returns>
        private async Task<IActionResult> SaveObstacleAndCreateReport(
            ObstacleDataViewModel viewModel,
            Func<ObstacleData, string> generateReportMessage)
        {
            // Map ViewModel to Entity
            var obstacleData = _obstacleRepository.MapFromViewModel(viewModel);
            
            // Set owner of the obstacle (logged-in pilot)
            var currentUser = await _userRepository.GetUserAsync(User);
            if (currentUser != null)
            {
                _obstacleRepository.SetObstacleOwner(obstacleData, currentUser.Id);
            }

            // Save the obstacle
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacleData);

            // Create automatic report when obstacle is created
            await _registrarRepository.AddRapportToObstacle(savedObstacle.ObstacleId, generateReportMessage(savedObstacle));

            // Map back to ViewModel for response
            var savedViewModel = _obstacleRepository.MapToViewModel(savedObstacle);
            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, savedViewModel);
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
        /// Helper method to return JSON if AJAX request, otherwise return view
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="viewModel">The saved obstacle ViewModel</param>
        /// <returns>JSON response if AJAX, otherwise returns overview view</returns>
        private IActionResult ReturnJsonOrViewIfAjax(int obstacleId, ObstacleDataViewModel viewModel)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = obstacleId }) });
            }
            return View("ObstacleOverview", viewModel);
        }

        /// <summary>
        /// Validates obstacle ViewModel for quick save (only geometry is required)
        /// </summary>
        /// <param name="viewModel">The obstacle ViewModel to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateQuickSave(ObstacleDataViewModel viewModel)
        {
            // Remove validation errors for fields that can be skipped in quick save
            var fieldsToSkip = new[] 
            { 
                nameof(ObstacleDataViewModel.ViewObstacleName), 
                nameof(ObstacleDataViewModel.ViewObstacleHeight), 
                nameof(ObstacleDataViewModel.ViewObstacleDescription) 
            };
            
            foreach (var field in fieldsToSkip)
            {
                ModelState.Remove(field);
            }

            // Validate only GeometryGeoJson (required field)
            if (string.IsNullOrEmpty(viewModel.ViewGeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleDataViewModel.ViewGeometryGeoJson), "Geometry (GeoJSON) is required.");
                return false;
            }

            return ModelState.IsValid;
        }

    }
}