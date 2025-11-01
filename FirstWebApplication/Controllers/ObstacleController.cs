using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // ObstacleController håndterer visning og innsending av data relatert til "ObstacleData"
    // Den viser et skjema (Dataform) og en oversikt (Overview) når skjemaet er sendt inn

    

    public class ObstacleController : Controller
    {
        // Return the partial form for AJAX or direct rendering
        private readonly ApplicationDBContext _context;

        public ObstacleController(ApplicationDBContext context)
        {
            _context = context;
        }
        public IActionResult DataFormPartial()
        {
            return PartialView("_ObstacleFormPartial", new ObstacleData());
        }


        // Handle the form submission from the partial form
        [HttpPost]
        public async Task<IActionResult> SubmitObstacle(ObstacleData obstacledata)
        {

            if (!ModelState.IsValid)
            {
                return PartialView("_ObstacleFormPartial", obstacledata);
            }
            _context.ObstaclesData.Add(obstacledata);
            await _context.SaveChangesAsync();

            return View("Overview", obstacledata);
        }
    }
}

