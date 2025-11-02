using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Registerfører")]
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