using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    public class RegistrarController : Controller
    {
        private readonly IObstacleRepository _obstacleRepository;
        private readonly IRegistrarRepository _registrarRepository;

        public RegistrarController(IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository)
        {
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
        }

        public async Task<IActionResult> Registrar()
        {
            var obstacles = await _obstacleRepository.GetAllObstacles();
            return View(obstacles);
        }

        [HttpPost]
        public IActionResult ShowObstacle(ObstacleData obstacledata)
        {
            return View("Registrar", obstacledata);
        }



        //public async Task<IActionResult> ObstacleUpdate()
        //{
        //    await _obstacleRepository.UpdateObstacles();
        //}
    }
}