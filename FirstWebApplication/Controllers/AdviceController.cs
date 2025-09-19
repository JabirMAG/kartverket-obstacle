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
        public IActionResult Advice(Advice Feedback)
        {
            if (!ModelState.IsValid)
            {
                return View(Feedback);
            }
            return View("Thank you", Feedback);
        }
    }

 }
