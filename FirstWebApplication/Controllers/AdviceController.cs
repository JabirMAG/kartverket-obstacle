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
        public IActionResult FeedbackForm(Advice Feedback)
        {
            return View();
        }

        // Behandler skjemaet når det sendes inn av brukeren (Post- forespørsel)
        [HttpPost]
        public async Task<IActionResult> FeedbackForm(AdviceViewModel requestData)
        {
            if (!ModelState.IsValid)
            {
                return View(requestData);
            }
            
            var advice = new Advice
            {
                adviceMessage = requestData.ViewadviceMessage,
                Email = requestData.ViewEmail,
            };

            await _adviceRepository.AddAdvice(advice);
            return View("ThankForm", advice);

        }

        // Viser en takk-side etter at tilbakemeldingen er sendt inn
        [HttpGet]
        public async Task<IActionResult> ThankForm(Advice adviceForm)
        {
            ViewBag.adviceMessage = adviceForm.adviceMessage;
            return View(adviceForm);
        }
    }

 }
