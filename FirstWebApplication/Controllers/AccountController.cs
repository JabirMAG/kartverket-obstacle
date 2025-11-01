using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;

namespace FirstWebApplication.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Skip all user/session checks and always redirect to Map page
            return RedirectToAction("Map", "Map");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
