using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FirstWebApplication.Controllers
{
    // Controller for autentisering og kontoadministrasjon. Håndterer brukerregistrering, innlogging, utlogging og passordtilbakestilling
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;

        // Konstant for rolle-navn - unngår hardkodede strenger og gjør koden mer vedlikeholdbar
        private const string RoleAdmin = "Admin";
        private const string RoleRegistrar = "Registerfører";
        
        // Konstant for omdirigeringshandlinger - sentraliserer omdirigeringsmål for enklere vedlikehold
        private const string ActionMap = "Map";
        private const string ActionDashboard = "Dashboard";
        private const string ActionRegistrar = "Registrar";
        private const string ControllerMap = "Map";
        private const string ControllerAdmin = "Admin";
        private const string ControllerRegistrar = "Registrar";
        
        // Feilmeldinger - sentraliserer meldinger for konsistens
        private const string ErrorInvalidCredentials = "Ugyldig brukernavn eller passord.";
        private const string ErrorAccountNotApproved = "Din konto er ikke godkjent ennå. Vent til en administrator har godkjent kontoen din.";
        private const string ErrorInvalidResetLink = "Ugyldig tilbakestillingslink.";
        private const string ErrorInvalidOrganization = "Velg en gyldig organisasjon.";

        public AccountController(
            IUserRepository userRepository, 
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _environment = environment;
            _logger = logger;
        }
        
        
        // Viser brukerregistreringsskjemaet
        [HttpGet]
        public IActionResult Register()
        {
            PopulateRegisterViewData();
            return View();
        }
        
        // Behandler brukerregistrering. Nye brukere må godkjennes av en administrator før de kan logge inn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            // Valider og normaliser organisasjon (case-insensitive matching)
            var normalizedOrganization = _userRepository.ValidateAndNormalizeOrganization(registerViewModel.Organization);
            if (normalizedOrganization == null)
            {
                ModelState.AddModelError(nameof(registerViewModel.Organization), ErrorInvalidOrganization);
            }
            else
            {
                registerViewModel.Organization = normalizedOrganization;
            }

            if (!ModelState.IsValid)
            {
                PopulateRegisterViewData();
                return View(registerViewModel);
            }
            
            // Opprett ny bruker - ikke godkjent før administrator godkjenner
            var newUser = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
                DesiredRole = registerViewModel.DesiredRole,
                IaApproved = false, // Krever administratorgodkjenning
                Organization = registerViewModel.Organization
            };
            
            var createResult = await _userRepository.CreateAsync(newUser, registerViewModel.Password);

            if (!createResult.Succeeded)
            {
                // Legg til feilmeldinger fra Identity (passordkrav, etc.)
                AddIdentityErrorsToModelState(createResult.Errors, nameof(registerViewModel.Password));
                PopulateRegisterViewData();
                return View(registerViewModel);
            }

            // Suksess - omdiriger til bekreftelsesside
            TempData["Message"] = 
                "Takk for registreringen! Du vil motta e-post når en administrator har godkjent kontoen din";
            return RedirectToAction("RegisterConfirmation");
        }
        
        // Viser registreringsbekreftelsesside
        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }
        
        // Viser innloggingsskjemaet. Hvis brukeren allerede er innlogget, omdirigeres til rolle-spesifikk visning.
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userRepository.GetUserAsync(User);
                if (user != null)
                {
                    return await RedirectToRoleBasedActionAsync(user);
                }
            }
            
            return View();
        }
        
        // Behandler brukerinnlogging. Kun godkjente brukere kan logge inn. Omdirigerer til passende side basert på brukerens rolle.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Finn bruker og valider at den eksisterer og er godkjent
            var user = await _userRepository.GetByNameAsync(model.Username);
            if (user == null || !user.IaApproved)
            {
                // Bruk generisk feilmelding for å unngå å avsløre om bruker eksisterer
                if (user != null && !user.IaApproved)
                {
                    ModelState.AddModelError(string.Empty, ErrorAccountNotApproved);
                }
                else
                {
                    AddLoginError();
                }
                return View(model);
            }

            // Forsøk å logge inn
            var signInResult = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                model.Password, 
                isPersistent: false, 
                lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                AddLoginError();
                return View(model);
            }

            return await RedirectToRoleBasedActionAsync(user);
        }
        
        // Logger ut nåværende bruker og tømmer sesjonen
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        // Viser skjemaet for glemt passord
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        // Starter passordtilbakestillingsprosessen. Genererer en tilbakestillingsnøkkel og sender den via e-post (i produksjon).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetByEmailAsync(model.Email);
            // Ikke avslør at bruker ikke eksisterer - omdiriger alltid til bekreftelse
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generer og koder tilbakestillingsnøkkel
            var encodedToken = await _userRepository.GenerateEncodedPasswordResetTokenAsync(user);
            
            // Opprett tilbakestillings-URL
            var callbackUrl = CreatePasswordResetUrl(encodedToken, user.Email!);

            // Logg i utviklingsmiljø (i produksjon sendes dette via e-post)
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Password Reset Email for {Email}: {CallbackUrl}", user.Email, callbackUrl);
            }

            // TODO: Send e-post her via e-posttjeneste
            // I produksjon: implementer IEmailSender og send e-posten

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // Viser bekreftelsesside etter at passordtilbakestillingsforespørsel er sendt inn
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        
        // Viser passordtilbakestillingsskjemaet med den oppgitte nøkkelen og e-postadressen
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, ErrorInvalidResetLink);
                PopulatePasswordViewData();
                return View();
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            PopulatePasswordViewData();
            return View(model);
        }
        
        // Behandler passordtilbakestilling med den oppgitte nøkkelen og det nye passordet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                PopulatePasswordViewData();
                return View(model);
            }

            var user = await _userRepository.GetByEmailAsync(model.Email);
            // Ikke avslør at bruker ikke eksisterer - omdiriger alltid til bekreftelse
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userRepository.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddIdentityErrorsToModelState(result.Errors, nameof(model.Password));
            PopulatePasswordViewData();
            return View(model);
        }

        // Viser bekreftelsesside etter at passordet er tilbakestilt
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // Fyller ViewBag med data som trengs for registreringsskjemaet
        private void PopulateRegisterViewData()
        {
            PopulatePasswordViewData();
            ViewBag.OrganizationOptions = _userRepository.GetAllOrganizations();
        }

        // Fyller ViewBag med passordkrav og -policy for visning i skjema
        private void PopulatePasswordViewData()
        {
            var passwordOptions = _userRepository.GetPasswordOptions();
            ViewBag.PasswordRequirements = BuildPasswordRequirements(passwordOptions);
            ViewBag.PasswordPolicy = BuildPasswordPolicyObject(passwordOptions);
        }


        // Legger til Identity-feil i ModelState, skiller passordfeil fra andre feil
        private void AddIdentityErrorsToModelState(IEnumerable<IdentityError> errors, string passwordPropertyName)
        {
            var passwordOptions = _userRepository.GetPasswordOptions();
            var passwordErrors = errors
                .Where(e => IsPasswordError(e))
                .ToList();

            var otherErrors = errors.Except(passwordErrors).ToList();

            if (passwordErrors.Any())
            {
                ModelState.AddModelError(passwordPropertyName, 
                    "Passordet oppfyller ikke kravene. Følg kravene som vises under passordfeltet.");
                
                foreach (var error in passwordErrors)
                {
                    var friendlyMessage = TranslatePasswordError(error.Description, passwordOptions);
                    ModelState.AddModelError(passwordPropertyName, friendlyMessage);
                }
            }

            foreach (var error in otherErrors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Bestemmer om en Identity-feil er relatert til passordvalidering
        private static bool IsPasswordError(IdentityError error)
        {
            return (!string.IsNullOrEmpty(error.Code) && 
                    error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)) ||
                   (!string.IsNullOrEmpty(error.Description) && 
                    error.Description.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // Bygger en liste med passordkrav basert på konfigurerte innstillinger
        private static IEnumerable<string> BuildPasswordRequirements(PasswordOptions opts)
        {
            var requirements = new List<string>
            {
                $"Minimum lengde: {opts.RequiredLength} tegn"
            };

            // Legg til krav basert på konfigurasjon
            if (opts.RequireDigit)
                requirements.Add("Minst ett siffer (0-9)");
            
            if (opts.RequireLowercase)
                requirements.Add("Minst en liten bokstav (a-z)");
            
            if (opts.RequireUppercase)
                requirements.Add("Minst en stor bokstav (A-Z)");
            
            if (opts.RequireNonAlphanumeric)
                requirements.Add("Minst ett ikke-alfanumerisk tegn (f.eks. !, @, #)");
            
            if (opts.RequiredUniqueChars > 1)
                requirements.Add($"Minst {opts.RequiredUniqueChars} unike tegn");

            return requirements;
        }

        // Bygger et objekt med passordpolicy-innstillinger for bruk i frontend
        private static object BuildPasswordPolicyObject(PasswordOptions opts)
        {
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

        // Oversetter Identity-feilmeldinger til norske, brukervennlige meldinger
        private string TranslatePasswordError(string? description, PasswordOptions opts)
        {
            if (string.IsNullOrEmpty(description))
                return "Passordet oppfyller ett eller flere krav.";

            var lowerDescription = description.ToLowerInvariant();

            // Sjekk lengdekrav
            if (lowerDescription.Contains("at least") && lowerDescription.Contains("characters"))
            {
                return GetLengthErrorMessage(lowerDescription, opts.RequiredLength);
            }

            // Sjekk spesifikke tegnkrav
            if (IsUppercaseError(lowerDescription))
                return "Må inneholde minst en stor bokstav (A-Z).";

            if (IsLowercaseError(lowerDescription))
                return "Må inneholde minst en liten bokstav (a-z).";

            if (IsDigitError(lowerDescription))
                return "Må inneholde minst ett siffer (0-9).";

            if (IsNonAlphanumericError(lowerDescription))
                return "Må inneholde minst ett ikke-alfanumerisk tegn (f.eks. !, @, #).";

            if (lowerDescription.Contains("unique") || lowerDescription.Contains("unike"))
            {
                return GetUniqueCharsErrorMessage(lowerDescription, opts.RequiredUniqueChars);
            }

            // Fallback: returner original melding med prefiks
            return "Passordfeil: " + description;
        }

        // Trekker ut og formaterer lengdekrav-feilmelding
        private static string GetLengthErrorMessage(string description, int defaultLength)
        {
            var digits = Regex.Match(description, @"\d+").Value;
            var length = !string.IsNullOrEmpty(digits) ? digits : defaultLength.ToString();
            return $"Passordet må være minst {length} tegn langt.";
        }

        // Trekker ut og formaterer unike tegn-krav-feilmelding
        private static string GetUniqueCharsErrorMessage(string description, int defaultUniqueChars)
        {
            var digits = Regex.Match(description, @"\d+").Value;
            var uniqueChars = !string.IsNullOrEmpty(digits) ? digits : defaultUniqueChars.ToString();
            return $"Må inneholde minst {uniqueChars} unike tegn.";
        }

        // Sjekker om feilbeskrivelse indikerer krav om stor bokstav
        private static bool IsUppercaseError(string description)
        {
            return description.Contains("uppercase") || 
                   description.Contains("upper-case") || 
                   description.Contains("store bokstav");
        }

        // Sjekker om feilbeskrivelse indikerer krav om liten bokstav
        private static bool IsLowercaseError(string description)
        {
            return description.Contains("lowercase") || 
                   description.Contains("lower-case") || 
                   description.Contains("liten bokstav");
        }

        // Sjekker om feilbeskrivelse indikerer krav om siffer
        private static bool IsDigitError(string description)
        {
            return description.Contains("digit") || 
                   description.Contains("siffer") || 
                   description.Contains("number");
        }

        // Sjekker om feilbeskrivelse indikerer krav om ikke-alfanumerisk tegn
        private static bool IsNonAlphanumericError(string description)
        {
            return description.Contains("non alphanumeric") || 
                   description.Contains("non-alphanumeric") || 
                   description.Contains("ikke-alfanumerisk");
        }

        // Legger til standard feilmelding for ugyldig innlogging
        private void AddLoginError()
        {
            ModelState.AddModelError(string.Empty, ErrorInvalidCredentials);
        }


        // Oppretter full URL for passordtilbakestilling
        private string CreatePasswordResetUrl(string token, string email)
        {
            return Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new { token, email },
                protocol: Request.Scheme)!;
        }


        // Omdirigerer bruker til passende side basert på deres rolle
        private async Task<IActionResult> RedirectToRoleBasedActionAsync(ApplicationUser user)
        {
            if (user == null)
            {
                return RedirectToAction(ActionMap, ControllerMap);
            }

            if (await _userRepository.IsInRoleAsync(user, RoleAdmin))
            {
                return RedirectToAction(ActionDashboard, ControllerAdmin);
            }

            if (await _userRepository.IsInRoleAsync(user, RoleRegistrar))
            {
                return RedirectToAction(ActionRegistrar, ControllerRegistrar);
            }

            // Piloter og andre godkjente brukere går til kartside
            return RedirectToAction(ActionMap, ControllerMap);
        }
    }
}