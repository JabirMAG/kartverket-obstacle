using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;

        public RegisterController(IObstacleRepository obstacleRepository)
        {
            _obstacleRepository = obstacleRepository;
        }

        public async Task<IActionResult> Register()
        {
            var obstacles = await _obstacleRepository.GetAllObstacles();
            return View(obstacles);
        }

        [HttpPost]
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Register", obstacledata);
        }

        public async Task DeleteObstacleWithStatus()
        {
            await _obstacleRepository.DeleteObstacle();
        }

        //public async Task<IActionResult> ObstacleUpdate()
        //{
        //    await _obstacleRepository.UpdateObstacles();
        //}
    }
}