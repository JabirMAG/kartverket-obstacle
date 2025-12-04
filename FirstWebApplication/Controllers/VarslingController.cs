using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    // Controller for varslingsfunksjonalitet. Håndterer visning av kommentarer/rapporter på brukerens hindringer.
    // Kun tilgjengelig for Pilot-brukere.
    [Authorize(Roles = "Pilot")]
    public class VarslingController : Controller
    {
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IUserRepository _userRepository;

        // Konstant for rolle-navn
        private const string RolePilot = "Pilot";

        // Feilmeldinger
        private const string ErrorObstacleNotFound = "Fant ikke hindring.";

        public VarslingController(
            IRegistrarRepository registrarRepository,
            IObstacleRepository obstacleRepository,
            IUserRepository userRepository)
        {
            _registrarRepository = registrarRepository;
            _obstacleRepository = obstacleRepository;
            _userRepository = userRepository;
        }

        // Viser alle varslinger (kommentarer/rapporter) for hindringer eid av innlogget pilot. Filtrerer ut automatisk genererte kommentarer og grupperer etter hindring.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userRepository.GetUserAsync(User);
            if (user == null)
            {
                return View(new List<VarslingViewModel>());
            }

            var userObstacleIds = await _obstacleRepository.GetObstacleIdsByOwner(user.Id);
            var userComments = await _registrarRepository.GetRapportsForUserObstacles(userObstacleIds);
            
            SaveLastViewedRapportId(user.Id, userComments);
            
            var varslinger = await BuildVarslingViewModelsAsync(userComments);
            
            return View(varslinger);
        }

        // Viser detaljert visning av en hindring og dens rapporter for innlogget pilot.
        // Kun hindringer eid av innlogget pilot kan vises.
        [HttpGet]
        public async Task<IActionResult> Details(int obstacleId)
        {
            var user = await _userRepository.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var obstacle = await _obstacleRepository.GetObstacleByOwnerAndId(obstacleId, user.Id);
            if (obstacle == null)
            {
                TempData["Error"] = ErrorObstacleNotFound;
                return RedirectToAction(nameof(Index));
            }

            var obstacleRapports = await _registrarRepository.GetRapportsByObstacleId(obstacleId);
            
            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;
            
            return View(obstacle);
        }

        // Henter antall nye/uleste varslinger (kommentarer fra admin/registerfører) for nåværende pilot-bruker.
        // Returnerer 0 hvis bruker ikke er pilot eller ikke autentisert.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetNotificationCount()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { count = 0 });
            }

            try
            {
                var user = await _userRepository.GetUserAsync(User);
                if (user == null || !await _userRepository.IsInRoleAsync(user, RolePilot))
                {
                    return Json(new { count = 0 });
                }

                var lastViewedRapportId = GetLastViewedRapportId(user.Id);
                var userObstacleIds = await _obstacleRepository.GetObstacleIdsByOwner(user.Id);
                var unreadCount = await _registrarRepository.GetUnreadNotificationsCount(userObstacleIds, lastViewedRapportId);

                return Json(new { count = unreadCount });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        // Lagrer høyeste RapportID brukeren har sett til session
        private void SaveLastViewedRapportId(string userId, List<RapportData> userComments)
        {
            var maxRapportId = userComments.Any() ? userComments.Max(r => r.RapportID) : 0;
            HttpContext.Session.SetInt32($"LastViewedRapportId_{userId}", maxRapportId);
        }

        // Henter høyeste RapportID brukeren har sett fra session
        private int GetLastViewedRapportId(string userId)
        {
            return HttpContext.Session.GetInt32($"LastViewedRapportId_{userId}") ?? 0;
        }

        // Bygger ViewModel-liste for varslinger gruppert etter hindring.
        // Bruker hindringer allerede inkludert i kommentarene for å unngå N+1 spørringsproblem.
        private Task<List<VarslingViewModel>> BuildVarslingViewModelsAsync(List<RapportData> userComments)
        {
            var groupedComments = userComments
                .Where(r => r.Obstacle != null)
                .GroupBy(r => r.ObstacleId)
                .ToList();

            var varslinger = new List<VarslingViewModel>();

            foreach (var group in groupedComments)
            {
                // Bruk hindring allerede inkludert i kommentaren for å unngå ekstra databasekall
                var firstComment = group.First();
                var obstacle = firstComment.Obstacle;

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

            var result = varslinger
                .OrderByDescending(v => v.Comments.FirstOrDefault()?.RapportID ?? 0)
                .ToList();

            return Task.FromResult(result);
        }


    }
}

