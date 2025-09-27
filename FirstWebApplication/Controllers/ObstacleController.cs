using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FirstWebApplication.Controllers
{
    // ObstacleController håndterer visning og innsending av data relatert til "ObstacleData"
    // Den viser et skjema (Dataform) og en oversikt (Overview) når skjemaet er sendt inn

    public class ObstacleController : Controller
    {

        // Viser skjemaet (Dataform) der brukeren kan fylle inn data
        [HttpGet]
        public IActionResult Dataform()
        {
            return View();
        }

        // Blir kalt etter at vi trykker på "Submit" knappen på Dataform viewwt
        [HttpPost]
        public IActionResult Dataform(ObstacleData obstacledata)
        {
            // Validerer dataene, og hvis gyldige, sendes brukeren videre til "Overview"-siden.
            if (!ModelState.IsValid)
            {
                return View(obstacledata);
            }
            return View("Overview", obstacledata);
        }
    }
}
