using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // ObstacleController håndterer visning og innsending av data relatert til "ObstacleData"
    // Den viser et skjema (Dataform) og en oversikt (Overview) når skjemaet er sendt inn

    

    public class ObstacleController : Controller
    {
        // Return the partial form for AJAX or direct rendering

        public IActionResult DataFormPartial()
        {
           return View();
        }
        [HttpPost]
        public async Task<IActionResult> DataForm(ObstacleDataViewModel obstacleDataViewModel)
        {
            ObstacleData obstacleDataDto = new ObstacleData
            {
                ObstacleDescription = obstacleDataViewModel.ViewObstacleDescription, 
                ObstacleHeight = obstacleDataViewModel.ViewObstacleHeight,
                ObstacleName = obstacleDataViewModel.ViewObstacleName
            };

            await _obstacleRepository.AddObstacleData(obstacleDataDto);
            return View();
        }


        // Handle the form submission from the partial form
        [HttpPost]
        public IActionResult SubmitObstacle(ObstacleData obstacledata)
        {

            if (!ModelState.IsValid)
            {
                return PartialView("_ObstacleFormPartial", obstacledata);
            }

            return View("Overview", obstacledata);
        }



    }
}

