using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FirstWebApplication.Controllers
{
    // Controller for hindringshåndtering. Håndterer opprettelse, visning og innsending av hindringer.
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

        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleDataViewModel());
        }

        //// Viser hindringen med gitt ID.
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

        // Hurtiglagrer hindringen med kun geometri og oppretter en automatisk rapport.
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

        // Sender inn en fullstendig utfylt hindring med alle påkrevde felt validert. Oppretter automatisk en rapportoppføring når hindringen lagres.
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

        // Lagrer hindringen, oppretter rapport og returnerer riktig respons.

        private async Task<IActionResult> SaveObstacleAndCreateReport(
            ObstacleDataViewModel viewModel,
            Func<ObstacleData, string> generateReportMessage)
        {
             var obstacleData = _obstacleRepository.MapFromViewModel(viewModel);
            
            // Setter innlogget bruker som eier av hindringen.

            var currentUser = await _userRepository.GetUserAsync(User);
            if (currentUser != null)
            {
                _obstacleRepository.SetObstacleOwner(obstacleData, currentUser.Id);
            }

            var savedObstacle = await _obstacleRepository.AddObstacle(obstacleData);

            // Oppretter automatisk rapport når hindring opprettes
            await _registrarRepository.AddRapportToObstacle(savedObstacle.ObstacleId, generateReportMessage(savedObstacle));

            var savedViewModel = _obstacleRepository.MapToViewModel(savedObstacle);
            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, savedViewModel);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
        }

        // Returnerer JSON ved AJAX-forespørsel, ellers vanlig visning.

        private IActionResult ReturnJsonOrViewIfAjax(int obstacleId, ObstacleDataViewModel viewModel)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = obstacleId }) });
            }
            return View("ObstacleOverview", viewModel);
        }

        // Validerer hindring for hurtiglagring der kun geometri er påkrevd.

        private bool ValidateQuickSave(ObstacleDataViewModel viewModel)
        {
        // Fjerner validering for felter som ikke er påkrevd ved hurtiglagring.

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

            // Validerer kun GeometryGeoJson (påkrevd felt)
            if (string.IsNullOrEmpty(viewModel.ViewGeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleDataViewModel.ViewGeometryGeoJson), "Geometri (GeoJSON) er påkrevd.");
                return false;
            }

            return ModelState.IsValid;
        }

    }
}
