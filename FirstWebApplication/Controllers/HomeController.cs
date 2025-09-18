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
            return View();
        }

        public IActionResult ThankForm()
        {
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
