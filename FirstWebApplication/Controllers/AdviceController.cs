using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using System.Reflection;

namespace FirstWebApplication.Controllers
{
    public class AdviceController : Controller
    {
        private readonly IAdviceRepository _adviceRepository;
        public AdviceController(IAdviceRepository adviceRepository) {
           
            _adviceRepository = adviceRepository;
        }

        [HttpGet]
        public async Task<ActionResult> FeedbackForm(Advice Feedback)
        {
            
            return View();
        }

        // Behandler skjemaet når det sendes inn av brukeren (Post- forespørsel)
        [HttpPost]
        public async Task<ActionResult> FeedbackForm(AdviceViewModel requestData)
        {
            Advice advice = new Advice
            {
                adviceMessage = requestData.ViewadviceMessage,
                Email = requestData.ViewEmail,
            };

            await _adviceRepository.AddAdvice(advice);
            return View();

        }

        // Viser en takk-side etter at tilbakemeldingen er sendt inn
        [HttpGet]
        public IActionResult ThankForm(Advice adviceForm)
        {
            return View(adviceForm);
        }
    }

 }
