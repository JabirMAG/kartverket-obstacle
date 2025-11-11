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
            // Hent alle rapporter
            var allRapports = await _registrarRepository.GetAllRapports();
            
            // Filtrer ut automatisk genererte kommentarer (de som starter med "Hindring 'X' ble sendt inn")
            var comments = allRapports
                .Where(r => !(r.RapportComment.StartsWith("Hindring '") && 
                             r.RapportComment.Contains(" ble sendt inn. Høyde:")))
                .ToList();
            
            // Grupper kommentarer etter ObstacleId
            var groupedComments = comments
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
            
            return View(varslinger);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int obstacleId)
        {
            // Placeholder - denne vil vises senere
            // Den skal vise overview av rapport lignende på DetaljerOmRapport.cshtml men med redigeringstilgang
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Index));
            }
            
            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();
            
            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;
            
            return View(obstacle);
        }

        /// <summary>
        /// Gets the count of new notifications (comments from admin/registerfører)
        /// </summary>
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
                var allRapports = await _registrarRepository.GetAllRapports();
                
                // Filter out automatic submission comments
                // These are comments that start with "Hindring '" and contain " ble sendt inn. Høyde:"
                var notifications = allRapports
                    .Where(r => !(r.RapportComment.StartsWith("Hindring '") && 
                                 r.RapportComment.Contains(" ble sendt inn. Høyde:")))
                    .ToList();

                // For now, return all non-automatic comments as notifications
                // In the future, this should filter by user and track read/unread status
                var count = notifications.Count;

                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
    }
}

