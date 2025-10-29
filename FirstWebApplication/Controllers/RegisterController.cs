using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
    }
}
