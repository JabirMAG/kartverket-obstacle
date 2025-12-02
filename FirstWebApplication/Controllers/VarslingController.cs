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
    /// <summary>
    /// Controller for notification functionality. Handles display of comments/reports on user's obstacles.
    /// Only accessible to Pilot users.
    /// </summary>
    [Authorize(Roles = "Pilot")]
    public class VarslingController : Controller
    {
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IUserRepository _userRepository;

        // Role name constant
        private const string RolePilot = "Pilot";

        // Error messages
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

        /// <summary>
        /// Displays all notifications (comments/reports) for obstacles owned by the logged-in pilot. Filters out auto-generated comments and groups by obstacle.
        /// </summary>
        /// <returns>The notifications view with grouped comments by obstacle</returns>
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

        /// <summary>
        /// Displays detailed view of an obstacle and its reports for the logged-in pilot.
        /// Only obstacles owned by the logged-in pilot can be viewed.
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to view</param>
        /// <returns>The obstacle details view, or redirects to index if obstacle not found or not owned by user</returns>
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

        /// <summary>
        /// Gets the count of new/unread notifications (comments from admin/registerfører) for the current pilot user.
        /// Returns 0 if user is not a pilot or not authenticated.
        /// </summary>
        /// <returns>JSON object with the notification count</returns>
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

        /// <summary>
        /// Saves the highest RapportID the user has seen to session
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="userComments">The list of comments the user has seen</param>
        private void SaveLastViewedRapportId(string userId, List<RapportData> userComments)
        {
            var maxRapportId = userComments.Any() ? userComments.Max(r => r.RapportID) : 0;
            HttpContext.Session.SetInt32($"LastViewedRapportId_{userId}", maxRapportId);
        }

        /// <summary>
        /// Gets the highest RapportID the user has seen from session
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The last viewed RapportID, or 0 if not set</returns>
        private int GetLastViewedRapportId(string userId)
        {
            return HttpContext.Session.GetInt32($"LastViewedRapportId_{userId}") ?? 0;
        }

        /// <summary>
        /// Builds ViewModel list for notifications grouped by obstacle.
        /// Uses obstacles already included in the comments to avoid N+1 query problem.
        /// </summary>
        /// <param name="userComments">The list of comments for user's obstacles (with obstacles already included)</param>
        /// <returns>List of VarslingViewModel grouped by obstacle</returns>
        private Task<List<VarslingViewModel>> BuildVarslingViewModelsAsync(List<RapportData> userComments)
        {
            var groupedComments = userComments
                .Where(r => r.Obstacle != null)
                .GroupBy(r => r.ObstacleId)
                .ToList();

            var varslinger = new List<VarslingViewModel>();

            foreach (var group in groupedComments)
            {
                // Use obstacle already included in the comment to avoid additional database calls
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

