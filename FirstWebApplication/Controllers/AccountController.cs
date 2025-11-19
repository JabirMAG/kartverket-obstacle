using FirstWebApplication.Constants;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
//using FirstWebApplication.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;

namespace FirstWebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _logger = logger;
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            PopulateRegisterViewData();
            return View();
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!OrganizationOptions.All.Contains(registerViewModel.Organization ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(registerViewModel.Organization), "Velg en gyldig organisasjon.");
            }
            else
            {
                registerViewModel.Organization = OrganizationOptions.All.First(o =>
                    string.Equals(o, registerViewModel.Organization, StringComparison.OrdinalIgnoreCase));
            }

            if (!ModelState.IsValid)
            {
                PopulateRegisterViewData();
                return View(registerViewModel);
            }
            
            //lager new user
            var applicationUser = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
                DesiredRole = registerViewModel.DesiredRole,
                IaApproved = false,
                Organization = registerViewModel.Organization
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
                // Detect password-related errors
                var pwErrors = identityResult.Errors.Where(e =>
                    (!string.IsNullOrEmpty(e.Code) && e.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(e.Description) && e.Description.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                if (pwErrors.Any())
                {
                    ModelState.AddModelError(nameof(registerViewModel.Password), "Passordet oppfyller ikke kravene. Følg kravene som vises under passordfeltet.");
                    foreach (var pe in pwErrors)
                    {
                        var friendly = TranslatePasswordError(pe.Description, _userManager.Options.Password);
                        ModelState.AddModelError(nameof(registerViewModel.Password), friendly);
                    }
                }

                // Keep other identity errors as form-level messages
                var otherErrors = identityResult.Errors.Except(pwErrors).ToList();
                foreach (var err in otherErrors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }

                PopulateRegisterViewData();
                return View(registerViewModel);
            }
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find user by username only
            var user = await _userManager.FindByNameAsync(model.Username);

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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Create reset password URL
            var callbackUrl = Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new { token, email = user.Email },
                protocol: Request.Scheme);

            // In development, log the email content
            // In production, you would send this via email service
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Password Reset Email for {Email}: {CallbackUrl}", user.Email, callbackUrl);
            }

            // TODO: Send email here using your email service
            // For now, we'll just redirect to confirmation page
            // In production, implement IEmailSender and send the email

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Ugyldig tilbakestillingslink.");
                ViewBag.PasswordRequirements = BuildPasswordRequirements();
                ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            ViewBag.PasswordRequirements = BuildPasswordRequirements();
            ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            // Decode the token
            var tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
            var token = Encoding.UTF8.GetString(tokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            // Detect password-related errors
            var pwErrors = result.Errors.Where(e =>
                (!string.IsNullOrEmpty(e.Code) && e.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(e.Description) && e.Description.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            if (pwErrors.Any())
            {
                ModelState.AddModelError(nameof(model.Password), "Passordet oppfyller ikke kravene. Følg kravene som vises under passordfeltet.");
                foreach (var pe in pwErrors)
                {
                    var friendly = TranslatePasswordError(pe.Description, _userManager.Options.Password);
                    ModelState.AddModelError(nameof(model.Password), friendly);
                }
            }

            // Keep other identity errors as form-level messages
            var otherErrors = result.Errors.Except(pwErrors).ToList();
            foreach (var err in otherErrors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            ViewBag.PasswordRequirements = BuildPasswordRequirements();
            ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private void PopulateRegisterViewData()
        {
            ViewBag.PasswordRequirements = BuildPasswordRequirements();
            ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
            ViewBag.OrganizationOptions = OrganizationOptions.All;
        }

        private IEnumerable<string> BuildPasswordRequirements()
        {
            var opts = _userManager.Options.Password;
            var reqs = new List<string>
            {
                $"Minimum lengde: {opts.RequiredLength} tegn"
            };

            if (opts.RequireDigit) reqs.Add("Minst ett siffer (0-9)");
            if (opts.RequireLowercase) reqs.Add("Minst en liten bokstav (a-z)");
            if (opts.RequireUppercase) reqs.Add("Minst en stor bokstav (A-Z)");
            if (opts.RequireNonAlphanumeric) reqs.Add("Minst ett ikke-alfanumerisk tegn (f.eks. !, @, #)");
            if (opts.RequiredUniqueChars > 1) reqs.Add($"Minst {opts.RequiredUniqueChars} unike tegn");

            return reqs;
        }

        private object BuildPasswordPolicyObject()
        {
            var opts = _userManager.Options.Password;
            return new
            {
                opts.RequiredLength,
                opts.RequireDigit,
                opts.RequireLowercase,
                opts.RequireUppercase,
                opts.RequireNonAlphanumeric,
                opts.RequiredUniqueChars
            };
        }

        private string TranslatePasswordError(string? description, PasswordOptions opts)
        {
            if (string.IsNullOrEmpty(description))
                return "Passordet oppfyller ett eller flere krav.";

            var d = description.ToLowerInvariant();

            if (d.Contains("at least") && d.Contains("characters"))
            {
                var digits = System.Text.RegularExpressions.Regex.Match(d, @"\d+").Value;
                if (!string.IsNullOrEmpty(digits))
                    return $"Passordet må være minst {digits} tegn langt.";
                return $"Passordet må være minst {opts.RequiredLength} tegn langt.";
            }

            if (d.Contains("uppercase") || d.Contains("upper-case") || d.Contains("store bokstav"))
                return "Må inneholde minst en stor bokstav (A-Z).";

            if (d.Contains("lowercase") || d.Contains("lower-case") || d.Contains("liten bokstav"))
                return "Må inneholde minst en liten bokstav (a-z).";

            if (d.Contains("digit") || d.Contains("siffer") || d.Contains("number"))
                return "Må inneholde minst ett siffer (0-9).";

            if (d.Contains("non alphanumeric") || d.Contains("non-alphanumeric") || d.Contains("ikke-alfanumerisk"))
                return "Må inneholde minst ett ikke-alfanumerisk tegn (f.eks. !, @, #).";

            if (d.Contains("unique") || d.Contains("unike"))
            {
                var digits = System.Text.RegularExpressions.Regex.Match(d, @"\d+").Value;
                if (!string.IsNullOrEmpty(digits))
                    return $"Må inneholde minst {digits} unike tegn.";
                return $"Må inneholde minst {opts.RequiredUniqueChars} unike tegn.";
            }

            return "Passordfeil: " + description;
        }
    }
}
