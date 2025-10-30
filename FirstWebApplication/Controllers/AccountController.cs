using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using System.Reflection;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        
        public AccountController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;    
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
            };

            var identityResult = await userManager.CreateAsync(identityUser, registerViewModel.Password); //lager bruker

            if (identityResult.Succeeded)
            {
                //assign this user the "User" role
                var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

                if (roleIdentityResult.Succeeded)
                {
                    //show success notification
                    return RedirectToAction("Register"); 
                }
            }
            //show error notification
            return View();
        }
    }
}




