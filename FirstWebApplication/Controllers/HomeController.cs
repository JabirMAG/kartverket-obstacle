using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string? _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IConfiguration config, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Hvis brukeren er innlogget, redirect til deres hjemmeside basert på rolle
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Pilot"))
                    {
                        return RedirectToAction("Map", "Map");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Registerfører"))
                    {
                        return RedirectToAction("Registrar", "Registrar");
                    }
                }
            }

            // Hvis ikke innlogget, vis forsiden
            var hour = DateTime.Now.Hour;
            string greeting;

            if (hour < 12)
                greeting = "God morgen!";
            else if (hour < 18)
                greeting = "God ettermiddag!";
            else
            
                greeting = "God kveld!";

            ViewBag.Greeting = greeting;
            return View();
        }

        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacledata)
        {
            return View("~/Views/Pilot/Overview.cshtml", obstacledata);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult OmOss()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
