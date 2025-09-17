using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        [HttpGet]
        public IActionResult Dataform()
        {
            return View();
        }

        // Blir kalt etter at vi trykker på "Submit" knappen på Dataform viewwt
        [HttpPost]
        public IActionResult Dataform(ObstacleData obstacledata)
        {
            if (!ModelState.IsValid)
            {
                return View(obstacledata);
            }
            return View("Overview", obstacledata);
        }
    }
}

