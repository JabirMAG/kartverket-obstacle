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
        public AdviceController(IAdviceRepository adviceRepository) {
           
            _adviceRepository = adviceRepository;
        }

        /// <summary>
        /// Displays the feedback form
        /// </summary>
        /// <returns>The feedback form view</returns>
        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View();
        }

        /// <summary>
        /// Processes the feedback form submission from the user
        /// </summary>
        /// <param name="requestData">The feedback data from the form</param>
        /// <returns>Redirects to thank you page on success, or returns feedback form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        /// <summary>
        /// Displays a thank you page after feedback has been submitted
        /// </summary>
        /// <param name="adviceForm">The advice data to display</param>
        /// <returns>The thank you view</returns>
        [HttpGet]
        public IActionResult ThankForm(Advice adviceForm)
        {
            ViewBag.adviceMessage = adviceForm.adviceMessage;
            return View(adviceForm);
        }
    }

 }
