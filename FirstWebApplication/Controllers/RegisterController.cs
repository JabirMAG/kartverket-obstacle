using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Register()
        {
            var obstacles = ObstacleRepository.GetAllObstacles();
            return View(obstacles);

        }


        // Handle the form submission from the partial form
        [HttpPost]
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Register", obstacledata);
        }
    }
}
