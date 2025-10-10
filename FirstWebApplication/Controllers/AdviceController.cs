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

        private readonly IAdviceRepository _iAdvicerepository;

        public AdviceController (IAdviceRepository _iadviceRepository)
        {
            _iAdvicerepository = _iadviceRepository;
        }
    
       
      
        [HttpGet]
        public async Task<ActionResult> FeedbackForm(Advice Feedback)
        {
            
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> FeedbackForm(AdviceViewModel requestData)
        {

            Advice advice = new Advice
            {

                adviceMessage = requestData.ViewadviceMessage,
                Email = requestData.ViewEmail,
                adviceID = requestData.ViewadviceID
            };

           await _iAdvicerepository.AddAdvice(advice);
            return View(advice);

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
