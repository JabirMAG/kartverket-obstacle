using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string? _connectionString;

        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
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
            return View("Overview", obstacledata);
        }

        public IActionResult Privacy()
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
