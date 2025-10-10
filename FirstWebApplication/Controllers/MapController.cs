using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class MapController : Controller
    {
        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }
    }
}
