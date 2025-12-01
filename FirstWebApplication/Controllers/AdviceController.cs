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
        /// <param name="model">The feedback data from the form</param>
        /// <returns>Redirects to thank you page on success, or returns feedback form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedbackForm(AdviceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var advice = MapToAdvice(model);
            await _adviceRepository.AddAdvice(advice);
            
            return RedirectToAction(nameof(ThankForm), new { email = model.ViewEmail, message = model.ViewadviceMessage });
        }

        /// <summary>
        /// Displays a thank you page after feedback has been submitted
        /// </summary>
        /// <param name="email">The email address from the feedback</param>
        /// <param name="message">The advice message from the feedback</param>
        /// <returns>The thank you view</returns>
        [HttpGet]
        public IActionResult ThankForm(string email, string message)
        {
            // Validate that required parameters are provided
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                // Redirect to feedback form if data is missing
                return RedirectToAction(nameof(FeedbackForm));
            }

            var advice = new Advice
            {
                Email = email,
                adviceMessage = message
            };

            return View(advice);
        }

        /// <summary>
        /// Maps AdviceViewModel to Advice model
        /// </summary>
        /// <param name="model">The ViewModel containing feedback data</param>
        /// <returns>Advice entity ready for database storage</returns>
        private static Advice MapToAdvice(AdviceViewModel model)
        {
            return new Advice
            {
                adviceMessage = model.ViewadviceMessage,
                Email = model.ViewEmail
            };
        }
    }
}
