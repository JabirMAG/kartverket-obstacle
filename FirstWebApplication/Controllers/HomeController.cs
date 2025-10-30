using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    // HomeController styrer standard sider som Index, Privacy og Error
    // Den er også koblet til en database-tilkobling via connection string
    //
    public class HomeController : Controller
    {
        // Logger brukes til å logge informasjon, advarsler og feil
        private readonly ILogger<HomeController> _logger;

        //Connection string til databasen (hentes fra appsettings.json)
        private readonly string? _connectionString;

        // Konstruktør som setter connection string og logger via dependency injection
        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        

        
        public IActionResult Index()
        {
            //Dynamisk innhold basert på tid
            var hour = DateTime.Now.Hour;
            string greeting;

            if (hour < 12)
                greeting = "God morgen!";
            else if (hour < 18)
                greeting = "God ettermiddag!";
            else
                greeting = "God kveld!";

            // Legger hilsen i ViewBag så view kan bruke det
            ViewBag.Greeting = greeting;

            return View();
        }
        
        [HttpGet]
        public IActionResult DataForm()
        {
            return View();
        }

        // Viser Privacy-siden (personvern)
        [HttpPost]
        public ActionResult DataForm(ObstacleData obstacledata)
        {
            return View("Overview", obstacledata);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Viser en feilmeldingsside dersom applikasjonen krasjer eller får en uventet feil.

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Oppretter et ErrorViewModel-objekt med RequestId for feilsøking
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
