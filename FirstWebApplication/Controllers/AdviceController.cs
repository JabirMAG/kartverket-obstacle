using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // Controller som håndterer tilbakemeldingsskjemaet
    public class AdviceController : Controller
    {
        // Viser skjemaet der brukeren kan sende tilbakemelding (Get- forespørsel)
        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View();
        }

        // Behandler skjemaet når det sendes inn av brukeren (Post- forespørsel)
        [HttpPost]
        public IActionResult FeedbackForm(Advice feedback)
        {
            // Sjekker om modellen er gyldig (at alle kravene er oppfylt)
            if (ModelState.IsValid)
            {
                // Sender brukeren til en takk-side med de innsendte dataene
                return RedirectToAction("ThankForm", feedback);
            }
            // Retunerer feilmelding hvis noe er galt med skjemaet
            return BadRequest("Feil i nettsiden");
        }

        // Viser en takk-side etter at tilbakemeldingen er sendt inn
        [HttpGet]
        public IActionResult ThankForm(Advice adviceForm)
        {
            return View(adviceForm);
        }
    }

 }
