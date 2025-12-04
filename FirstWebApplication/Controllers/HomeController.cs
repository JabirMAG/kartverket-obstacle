using System.Diagnostics;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // Controller for hjemmeside og generell nettstedfunksjonalitet
    public class HomeController : Controller
    {
        private readonly IGreetingRepository _greetingRepository;

        public HomeController(IGreetingRepository greetingRepository)
        {
            _greetingRepository = greetingRepository;
        }

        // Viser hjemmesiden med en tidsbasert hilsen
        public IActionResult Index()
        {
            ViewBag.Greeting = _greetingRepository.GetTimeBasedGreeting();
            return View();
        }

        // Viser personvernsiden
        public IActionResult Privacy()
        {
            return View();
        }

        // Viser om oss-siden
        public IActionResult OmOss()
        {
            return View();
        }

        // Viser feilside med feildetaljer
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
