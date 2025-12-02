using System.Diagnostics;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for home page and general site functionality
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IGreetingRepository _greetingRepository;

        public HomeController(IGreetingRepository greetingRepository)
        {
            _greetingRepository = greetingRepository;
        }

        /// <summary>
        /// Displays the home page with a time-based greeting
        /// </summary>
        /// <returns>The home page view</returns>
        public IActionResult Index()
        {
            ViewBag.Greeting = _greetingRepository.GetTimeBasedGreeting();
            return View();
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
