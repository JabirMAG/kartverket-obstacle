using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class AdviceController : Controller
    {

        [HttpGet]
        public IActionResult FeedbackForm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FeedbackForm(Advice feedback)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("ThankForm", feedback);
            }
            return BadRequest("Feil i nettsiden");
        }

        [HttpGet]
        public IActionResult ThankForm(Advice adviceForm)
        {
            return View(adviceForm);
        }

        

        [HttpPost]
        public async Task<IActionResult> Advice(Advice Feedback)
        {
            return View();
        }
    }

 }
