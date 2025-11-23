using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for home page and general site functionality
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string? _connectionString;

        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        /// <summary>
        /// Displays the home page with a time-based greeting
        /// </summary>
        /// <returns>The home page view</returns>
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

        /// <summary>
        /// Displays the data form view
        /// </summary>
        /// <returns>The data form view</returns>
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        /// <summary>
        /// Processes the data form submission
        /// </summary>
        /// <param name="obstacledata">The obstacle data from the form</param>
        /// <returns>The overview view with obstacle data</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DataForm(ObstacleData obstacledata)
        {
            return View("Overview", obstacledata);
        }

        /// <summary>
        /// Displays the privacy page
        /// </summary>
        /// <returns>The privacy view</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the about us page
        /// </summary>
        /// <returns>The about us view</returns>
        public IActionResult OmOss()
        {
            return View();
        }
        

        /// <summary>
        /// Displays error page with error details
        /// </summary>
        /// <returns>The error view with error details</returns>
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
