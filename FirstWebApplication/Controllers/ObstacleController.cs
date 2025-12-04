using FirstWebApplication.Repositories;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FirstWebApplication.Controllers
{
    // Controller for hindringsadministrasjon. Håndterer opprettelse, visning og innsending av hindringer.
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

        // Returnerer hindringsskjemaet som en delvis visning
        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleDataViewModel());
        }

        // Viser oversikt over en spesifikk hindring
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

        // Hurtiglagrer en hindring med minimalt påkrevd data (kun geometri er påkrevd). Oppretter automatisk en rapportoppføring når hindringen lagres.
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

        // Hjelpemetode for å lagre hindring, opprette rapport og returnere passende respons
        private async Task<IActionResult> SaveObstacleAndCreateReport(
            ObstacleDataViewModel viewModel,
            Func<ObstacleData, string> generateReportMessage)
        {
            // Map ViewModel til Entity
            var obstacleData = _obstacleRepository.MapFromViewModel(viewModel);
            
            // Sett eier av hindringen (innlogget pilot)
            var currentUser = await _userRepository.GetUserAsync(User);
            if (currentUser != null)
            {
                _obstacleRepository.SetObstacleOwner(obstacleData, currentUser.Id);
            }

            // Lagre hindringen
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacleData);

            // Opprett automatisk rapport når hindring opprettes
            await _registrarRepository.AddRapportToObstacle(savedObstacle.ObstacleId, generateReportMessage(savedObstacle));

            // Map tilbake til ViewModel for respons
            var savedViewModel = _obstacleRepository.MapToViewModel(savedObstacle);
            return ReturnJsonOrViewIfAjax(savedObstacle.ObstacleId, savedViewModel);
        }

        // Hjelpemetode for å sjekke om forespørsel er AJAX
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
        }


        // Hjelpemetode for å returnere JSON hvis AJAX-forespørsel, ellers returner view
        private IActionResult ReturnJsonOrViewIfAjax(int obstacleId, ObstacleDataViewModel viewModel)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action("Overview", "Obstacle", new { id = obstacleId }) });
            }
            return View("ObstacleOverview", viewModel);
        }

        // Validerer hindring ViewModel for hurtiglagring (kun geometri er påkrevd)
        private bool ValidateQuickSave(ObstacleDataViewModel viewModel)
        {
            // Fjern valideringsfeil for felt som kan hoppes over i hurtiglagring
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

            // Valider kun GeometryGeoJson (påkrevd felt)
            if (string.IsNullOrEmpty(viewModel.ViewGeometryGeoJson))
            {
                ModelState.AddModelError(nameof(ObstacleDataViewModel.ViewGeometryGeoJson), "Geometri (GeoJSON) er påkrevd.");
                return false;
            }

            return ModelState.IsValid;
        }

    }
}