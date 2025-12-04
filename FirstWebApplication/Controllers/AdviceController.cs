using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    // Controller for tilbakemelding/råd-funksjonalitet. Håndterer innsending og visning av brukertilbakemeldinger.
    public class AdviceController : Controller
    {
        private readonly IAdviceRepository _adviceRepository;

        public AdviceController(IAdviceRepository adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        // Viser tilbakemeldingsskjemaet
        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View(new AdviceViewModel());
        }

        // Behandler innsending av tilbakemeldingsskjemaet fra brukeren
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedbackForm(AdviceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var advice = _adviceRepository.MapFromViewModel(model);
            await _adviceRepository.AddAdvice(advice);
            
            return RedirectToAction(nameof(ThankForm), new { email = model.ViewEmail, message = model.ViewadviceMessage });
        }

        // Viser en takkside etter at tilbakemelding er sendt inn
        [HttpGet]
        public IActionResult ThankForm(string email, string message)
        {
            // Valider at påkrevde parametere er oppgitt
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                // Omdiriger til tilbakemeldingsskjema hvis data mangler
                return RedirectToAction(nameof(FeedbackForm));
            }

            var advice = _adviceRepository.CreateFromEmailAndMessage(email, message);

            return View(advice);
        }
    }
}
