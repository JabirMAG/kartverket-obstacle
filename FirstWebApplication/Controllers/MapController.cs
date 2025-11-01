using Microsoft.AspNetCore.Mvc;
using FirstWebApplication.Models;

namespace FirstWebApplication.Controllers
{
    public class MapController : Controller
    {
        [HttpGet]
        public IActionResult Map()
        {
            // Optional: send empty model or default data
            var obstacleData = new ObstacleData();
            return View(obstacleData);
        }
    }
}
