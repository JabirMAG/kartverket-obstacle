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


     

       public IActionResult Index()
        {
            //Dynamisk innhold basert på tid
            var hour = DateTime.Now.Hour;
            string greeting;
            
            if (hour < 12)
                greeting = "Good Morning!";
            else if (hour < 18)
                greeting = "Good Afternoon!";
            else 
                greeting = "Good Evening!";
                
            // Legger hilsen i ViewBag så view kan bruke det
            ViewBag.Greeting = greeting;
            
            return View();
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
