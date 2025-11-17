using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

    namespace FirstWebApplication.Controllers
{
    [Authorize]
    public class VarslingController : Controller
    {
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IObstacleRepository _obstacleRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public VarslingController(
            IRegistrarRepository registrarRepository,
            IObstacleRepository obstacleRepository,
            UserManager<ApplicationUser> userManager)
        {
            _registrarRepository = registrarRepository;
            _obstacleRepository = obstacleRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Hent innlogget bruker
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return View(new List<VarslingViewModel>());
            }

            // Hent alle rapporter
            var allRapports = await _registrarRepository.GetAllRapports();
            
            // Filtrer ut automatisk genererte kommentarer (de som starter med "Hindring 'X' ble sendt inn")
            var comments = allRapports
                .Where(r => !(r.RapportComment.StartsWith("Hindring '") && 
                             r.RapportComment.Contains(" ble sendt inn. Høyde:")))
                .ToList();
            
            // Hent hindringer som tilhører den innloggede brukeren
            var userObstacles = await _obstacleRepository.GetObstaclesByOwner(currentUser.Id);
            var userObstacleIds = userObstacles.Select(o => o.ObstacleId).ToHashSet();
            
            // Filtrer kommentarer til kun hindringer som tilhører brukeren
            var userComments = comments
                .Where(r => r.Obstacle != null && userObstacleIds.Contains(r.Obstacle.ObstacleId))
                .ToList();
            
            // Lagre den høyeste RapportID brukeren har sett (markerer alle varslinger med lavere eller lik ID som sett)
            var maxRapportId = userComments.Any() ? userComments.Max(r => r.RapportID) : 0;
            HttpContext.Session.SetInt32($"LastViewedRapportId_{currentUser.Id}", maxRapportId);
            
            // Grupper kommentarer etter ObstacleId
            var groupedComments = userComments
                .Where(r => r.Obstacle != null)
                .GroupBy(r => r.ObstacleId)
                .ToList();
            
            // Opprett ViewModel for hver obstacle med kommentarer
            var varslinger = new List<VarslingViewModel>();
            
            foreach (var group in groupedComments)
            {
                var obstacleId = group.Key;
                var obstacle = await _obstacleRepository.GetElementById(obstacleId);
                
                if (obstacle != null)
                {
                    var obstacleComments = group.OrderByDescending(r => r.RapportID).ToList();
                    
                    varslinger.Add(new VarslingViewModel
                    {
                        Obstacle = obstacle,
                        CommentCount = obstacleComments.Count,
                        Comments = obstacleComments
                    });
                }
            }
            
            // Sorter etter nyeste kommentar først
            varslinger = varslinger
                .OrderByDescending(v => v.Comments.FirstOrDefault()?.RapportID ?? 0)
                .ToList();
            
            return View("~/Views/Pilot/Varsling.cshtml", varslinger);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int obstacleId)
        {
            // Redirect til PilotController.DetaljerOmRapport for å bruke samme view med redigeringsfunksjonalitet
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }
            
            // Sjekk at hindringen tilhører den innloggede brukeren
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || obstacle.OwnerUserId != currentUser.Id)
            {
                TempData["Error"] = "Du har ikke tilgang til denne hindringen.";
                return RedirectToAction(nameof(Index));
            }
            
            // Redirect til PilotController.DetaljerOmRapport som har redigeringsfunksjonalitet
            return RedirectToAction("DetaljerOmRapport", "Pilot", new { obstacleId });
        }

        /// <summary>
        /// Gets the count of new/unread notifications (comments from admin/registerfører) for the current user
        /// Only accessible to Pilots
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { count = 0 });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { count = 0 });
                }

                // Only Pilots should see notifications
                if (!await _userManager.IsInRoleAsync(currentUser, "Pilot"))
                {
                    return Json(new { count = 0 });
                }

                // Hent den høyeste RapportID brukeren har sett
                var lastViewedRapportId = HttpContext.Session.GetInt32($"LastViewedRapportId_{currentUser.Id}") ?? 0;

                // Hent alle rapporter
                var allRapports = await _registrarRepository.GetAllRapports();
                
                // Filtrer ut automatisk genererte kommentarer (de som starter med "Hindring 'X' ble sendt inn")
                var comments = allRapports
                    .Where(r => !(r.RapportComment.StartsWith("Hindring '") && 
                                 r.RapportComment.Contains(" ble sendt inn. Høyde:")))
                    .ToList();

                // Filtrer kun varslinger for hindringer som tilhører den innloggede brukeren
                var userObstacles = await _obstacleRepository.GetObstaclesByOwner(currentUser.Id);
                var userObstacleIds = userObstacles.Select(o => o.ObstacleId).ToHashSet();

                // Filtrer ut varslinger som brukeren allerede har sett (RapportID <= lastViewedRapportId)
                var unreadNotifications = comments
                    .Where(r => r.Obstacle != null && 
                               userObstacleIds.Contains(r.Obstacle.ObstacleId) &&
                               r.RapportID > lastViewedRapportId)
                    .ToList();

                var count = unreadNotifications.Count;

                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
    }
}

