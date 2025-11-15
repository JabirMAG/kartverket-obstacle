using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
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
        /*
        public async Task<ActionResult> FeedbackForm(Advice Feedback)
        {   
            return View();
        }
*/
        public IActionResult FeedbackForm()
        {
            return View();
        }
        // Behandler skjemaet når det sendes inn av brukeren (Post- forespørsel)
        [HttpPost]
        public async Task<ActionResult> FeedbackForm(AdviceViewModel requestData)
        {
            if (!ModelState.IsValid)
            {
                return View(requestData);
            }
            
            Advice advice = new Advice
            {
                adviceMessage = requestData.ViewadviceMessage,
                Email = requestData.ViewEmail,
            };

            await _adviceRepository.AddAdvice(advice);
            return RedirectToAction("ThankForm", new { advice.Email, advice.adviceMessage });

        }

        // Viser en takk-side etter at tilbakemeldingen er sendt inn
        [HttpGet]
        public IActionResult ThankForm(Advice adviceForm)
        {
            ViewBag.adviceMessage = adviceForm.adviceMessage;
            return View(adviceForm);
        }
    }

 }
