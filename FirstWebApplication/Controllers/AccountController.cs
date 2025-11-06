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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
                return View(registerViewModel);
            
            //lager new user
            var applicationUser = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
                
            };
            
            var identityResult = await _userManager.CreateAsync(applicationUser, registerViewModel.Password);

            if (identityResult.Succeeded)
            {
                await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                return RedirectToAction("Map", "Map"); 
            }
/*
            foreach (var error in Results.Error) //to do: fikse error for identityresult
            {
                ModelState.AddModelError("", error.Description);
            }
        */
/*
            //lager bruker

         
            {
                //assign this user the "User" role
                var roleIdentityResult = await _userManager.AddToRoleAsync(identityUser, "User");

                if (roleIdentityResult.Succeeded)
                {
                    //show success notification
                    return RedirectToAction("Register"); 
                }
            }
            //show error notification
            */
            return View();
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Skip all user/session checks and always redirect to Map page
            return RedirectToAction("Map", "Map");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
