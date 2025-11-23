using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class AdviceController : Controller
    {
        private readonly IAdviceRepository _adviceRepository;

        public AdviceController(IAdviceRepository adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

      
        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View(new AdviceViewModel());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedbackForm(AdviceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Advice advice = new Advice
            {
                adviceMessage = model.ViewadviceMessage,
                Email = model.ViewEmail
            };

            await _adviceRepository.AddAdvice(advice);

            return RedirectToAction("ThankForm", new
            {
                email = advice.Email,
                message = advice.adviceMessage
            });
        }

       
        [HttpGet]
        public IActionResult ThankForm(string email, string message)
        {
            var model = new Advice
            {
                Email = email,
                adviceMessage = message
            };

            return View(model);
        }
    }
}

