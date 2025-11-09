using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
//using FirstWebApplication.NewFolder;
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
                DesiredRole = registerViewModel.DesiredRole,
                IaApproved = false
            };
            
            var identityResult = await _userManager.CreateAsync(applicationUser, registerViewModel.Password);

            if (identityResult.Succeeded)
            {
                TempData["Message"] =
                    "Takk for registreringen! Du vil motta e-post når en administrator har godkjent kontoen din";
                return RedirectToAction("RegisterConfirmation");
               
            }
            else
            {
                foreach (var error in identityResult.Errors) 
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
    
                // returner viewet på nytt slik at feilmeldingene vises
                return View(registerViewModel);
            }
            return View();
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find user by username or email
            var user = await _userManager.FindByNameAsync(model.Username) 
                       ?? await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord.");
                return View(model);
            }

            // Check if user is approved before allowing login
            if (!user.IaApproved)
            {
                ModelState.AddModelError(string.Empty, "Din konto er ikke godkjent ennå. Vent til en administrator har godkjent kontoen din.");
                return View(model);
            }

            // Attempt to sign in
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                model.Password, 
                isPersistent: false, 
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Ugyldig brukernavn eller passord.");
                return View(model);
            }

            // Check user role and redirect to appropriate page
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            var isRegisterforer = await _userManager.IsInRoleAsync(user, "Registerfører");
            if (isRegisterforer)
            {
                return RedirectToAction("Registrar", "Registrar");
            }

            // Pilots and other approved users go to Map page (register obstacle site)
            return RedirectToAction("Map", "Map");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
