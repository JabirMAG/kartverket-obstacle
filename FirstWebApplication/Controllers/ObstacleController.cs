using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ObstacleController : Controller
    {
        // Return the partial form for AJAX or direct rendering
        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleData());
        }

        // Handle the form submission from the partial form
        [HttpPost]
        public IActionResult SubmitObstacle(ObstacleData obstacledata)
        {


            //if (!ModelState.IsValid)
            //{
            //    return PartialView("_ObstacleFormPartial", obstacledata);
            //}

            return View("Overview", obstacledata);
        }



    }
}

