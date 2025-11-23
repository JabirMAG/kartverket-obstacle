using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for feedback/advice functionality. Handles submission and viewing of user feedback.
    /// </summary>
    public class AdviceController : Controller
    {
        private readonly IAdviceRepository _adviceRepository;

        public AdviceController(IAdviceRepository adviceRepository)
        {
            _adviceRepository = adviceRepository;
        }

        /// <summary>
        /// Displays the feedback form
        /// </summary>
        /// <returns>The feedback form view</returns>
        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View(new AdviceViewModel());
        }

        /// <summary>
        /// Processes the feedback form submission from the user
        /// </summary>
        /// <param name="requestData">The feedback data from the form</param>
        /// <returns>Redirects to thank you page on success, or returns feedback form with errors</returns>
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

        /// <summary>
        /// Displays a thank you page after feedback has been submitted
        /// </summary>
        /// <param name="adviceForm">The advice data to display</param>
        /// <returns>The thank you view</returns>
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

