using System.Diagnostics;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for home page and general site functionality
    /// </summary>
    public class HomeController : Controller
    {
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
