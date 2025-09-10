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

        // public HomeController(ILogger<HomeController> logger)
        // { 
        // _logger = logger;
        // }
        
        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        
        public async Task<IActionResult> Index()
        {
            string viewModel = "Connected successfuly to MariaDB!";
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                return Content("Failed to connect to MariaDB: " + ex.Message);
            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        // Blir kalt etter at vi trykker på "Register Obstacle" knappen

        [HttpGet]
        public IActionResult Dataform()
        {
            return View();
        }


        // Blir kalt etter at vi trykker på "Submit" knappen på Dataform viewwt
        [HttpPost]
        public IActionResult Dataform(ObstacleData obstacledata)
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
