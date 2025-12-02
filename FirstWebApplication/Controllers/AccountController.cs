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
    /// <summary>
    /// Controller for authentication and account management. Handles user registration, login, logout, and password reset
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;

        // Role name constants - avoids hardcoded strings and makes code more maintainable
        private const string RoleAdmin = "Admin";
        private const string RoleRegistrar = "Registerfører";
        
        // Redirect action constants - centralizes redirect targets for easier maintenance
        private const string ActionMap = "Map";
        private const string ActionDashboard = "Dashboard";
        private const string ActionRegistrar = "Registrar";
        private const string ControllerMap = "Map";
        private const string ControllerAdmin = "Admin";
        private const string ControllerRegistrar = "Registrar";
        
        // Error messages - centralizes messages for consistency
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
        
        
        /// <summary>
        /// Displays the user registration form
        /// </summary>
        /// <returns>The registration view</returns>
        [HttpGet]
        public IActionResult Register()
        {
            PopulateRegisterViewData();
            return View();
        }
        
        /// <summary>
        /// Processes user registration. New users must be approved by an admin before they can log in
        /// </summary>
        /// <param name="registerViewModel">The registration data from the form</param>
        /// <returns>Redirects to confirmation page on success, or returns the registration view with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            // Validate and normalize organization (case-insensitive matching)
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
            
            // Create new user - not approved until admin approves
            var newUser = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email,
                DesiredRole = registerViewModel.DesiredRole,
                IaApproved = false, // Requires admin approval
                Organization = registerViewModel.Organization
            };
            
            var createResult = await _userRepository.CreateAsync(newUser, registerViewModel.Password);

            if (!createResult.Succeeded)
            {
                // Add error messages from Identity (password requirements, etc.)
                AddIdentityErrorsToModelState(createResult.Errors, nameof(registerViewModel.Password));
                PopulateRegisterViewData();
                return View(registerViewModel);
            }

            // Success - redirect to confirmation page
            TempData["Message"] = 
                "Takk for registreringen! Du vil motta e-post når en administrator har godkjent kontoen din";
            return RedirectToAction("RegisterConfirmation");
        }
        
        /// <summary>
        /// Displays registration confirmation page
        /// </summary>
        /// <returns>The registration confirmation view</returns>
        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }
        
        /// <summary>
        /// Displays the login form. If user is already logged in, redirects to their role-specific view.
        /// </summary>
        /// <returns>The login view, or redirects to role-specific view if already logged in</returns>
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
        
        /// <summary>
        /// Processes user login. Only approved users can log in. Redirects to appropriate page based on user role.
        /// </summary>
        /// <param name="model">The login credentials</param>
        /// <returns>Redirects to appropriate page based on role, or returns login view with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find user and validate that it exists and is approved
            var user = await _userRepository.GetByNameAsync(model.Username);
            if (user == null || !user.IaApproved)
            {
                // Use generic error message to avoid revealing if user exists
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

            // Attempt to sign in
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
        
        /// <summary>
        /// Logs out the current user and clears the session
        /// </summary>
        /// <returns>Redirects to the home page</returns>
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        /// <summary>
        /// Displays the forgot password form
        /// </summary>
        /// <returns>The forgot password view</returns>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        /// <summary>
        /// Initiates password reset process. Generates a reset token and sends it via email (in production).
        /// </summary>
        /// <param name="model">The forgot password form data containing the email address</param>
        /// <returns>Redirects to confirmation page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetByEmailAsync(model.Email);
            // Don't reveal that user doesn't exist - always redirect to confirmation
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate and encode reset token
            var encodedToken = await _userRepository.GenerateEncodedPasswordResetTokenAsync(user);
            
            // Create reset URL
            var callbackUrl = CreatePasswordResetUrl(encodedToken, user.Email!);

            // Log in development (in production this is sent via email)
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("Password Reset Email for {Email}: {CallbackUrl}", user.Email, callbackUrl);
            }

            // TODO: Send email here via email service
            // In production: implement IEmailSender and send the email

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        /// <summary>
        /// Displays confirmation page after password reset request has been submitted
        /// </summary>
        /// <returns>The forgot password confirmation view</returns>
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        
        /// <summary>
        /// Displays the password reset form with the provided token and email
        /// </summary>
        /// <param name="token">The password reset token</param>
        /// <param name="email">The user's email address</param>
        /// <returns>The reset password view, or error view if token/email is invalid</returns>
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
        
        /// <summary>
        /// Processes password reset with the provided token and new password
        /// </summary>
        /// <param name="model">The reset password form data containing token, email, and new password</param>
        /// <returns>Redirects to confirmation page on success, or returns reset password view with errors</returns>
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
            // Don't reveal that user doesn't exist - always redirect to confirmation
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

        /// <summary>
        /// Displays confirmation page after password has been successfully reset
        /// </summary>
        /// <returns>The reset password confirmation view</returns>
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Populates ViewBag with data needed for the registration form
        /// </summary>
        private void PopulateRegisterViewData()
        {
            PopulatePasswordViewData();
            ViewBag.OrganizationOptions = _userRepository.GetAllOrganizations();
        }

        /// <summary>
        /// Populates ViewBag with password requirements and policy for display in form
        /// </summary>
        private void PopulatePasswordViewData()
        {
            var passwordOptions = _userRepository.GetPasswordOptions();
            ViewBag.PasswordRequirements = BuildPasswordRequirements(passwordOptions);
            ViewBag.PasswordPolicy = BuildPasswordPolicyObject(passwordOptions);
        }


        /// <summary>
        /// Adds Identity errors to ModelState, separating password errors from other errors
        /// </summary>
        /// <param name="errors">The Identity errors to add</param>
        /// <param name="passwordPropertyName">The name of the password property for error targeting</param>
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

        /// <summary>
        /// Determines if an Identity error is related to password validation
        /// </summary>
        /// <param name="error">The Identity error to check</param>
        /// <returns>True if the error is password-related, false otherwise</returns>
        private static bool IsPasswordError(IdentityError error)
        {
            return (!string.IsNullOrEmpty(error.Code) && 
                    error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)) ||
                   (!string.IsNullOrEmpty(error.Description) && 
                    error.Description.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Builds a list of password requirements based on configured settings
        /// </summary>
        /// <param name="opts">Password options configuration</param>
        /// <returns>List of password requirements to display to the user</returns>
        private static IEnumerable<string> BuildPasswordRequirements(PasswordOptions opts)
        {
            var requirements = new List<string>
            {
                $"Minimum lengde: {opts.RequiredLength} tegn"
            };

            // Add requirements based on configuration
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

        /// <summary>
        /// Builds an object with password policy settings for use in frontend
        /// </summary>
        /// <param name="opts">Password options configuration</param>
        /// <returns>Anonymous object with password policy settings</returns>
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

        /// <summary>
        /// Translates Identity error messages to Norwegian, user-friendly messages
        /// </summary>
        /// <param name="description">Original error message from Identity</param>
        /// <param name="opts">Password settings to retrieve default values</param>
        /// <returns>Norwegian, user-friendly error message</returns>
        private string TranslatePasswordError(string? description, PasswordOptions opts)
        {
            if (string.IsNullOrEmpty(description))
                return "Passordet oppfyller ett eller flere krav.";

            var lowerDescription = description.ToLowerInvariant();

            // Check length requirement
            if (lowerDescription.Contains("at least") && lowerDescription.Contains("characters"))
            {
                return GetLengthErrorMessage(lowerDescription, opts.RequiredLength);
            }

            // Check specific character requirements
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

            // Fallback: return original message with prefix
            return "Passordfeil: " + description;
        }

        /// <summary>
        /// Extracts and formats length requirement error message
        /// </summary>
        /// <param name="description">The error description containing length information</param>
        /// <param name="defaultLength">The default length if not found in description</param>
        /// <returns>Formatted Norwegian error message for length requirement</returns>
        private static string GetLengthErrorMessage(string description, int defaultLength)
        {
            var digits = Regex.Match(description, @"\d+").Value;
            var length = !string.IsNullOrEmpty(digits) ? digits : defaultLength.ToString();
            return $"Passordet må være minst {length} tegn langt.";
        }

        /// <summary>
        /// Extracts and formats unique characters requirement error message
        /// </summary>
        /// <param name="description">The error description containing unique characters information</param>
        /// <param name="defaultUniqueChars">The default unique characters count if not found in description</param>
        /// <returns>Formatted Norwegian error message for unique characters requirement</returns>
        private static string GetUniqueCharsErrorMessage(string description, int defaultUniqueChars)
        {
            var digits = Regex.Match(description, @"\d+").Value;
            var uniqueChars = !string.IsNullOrEmpty(digits) ? digits : defaultUniqueChars.ToString();
            return $"Må inneholde minst {uniqueChars} unike tegn.";
        }

        /// <summary>
        /// Checks if error description indicates uppercase letter requirement
        /// </summary>
        /// <param name="description">The error description to check</param>
        /// <returns>True if error is about uppercase requirement</returns>
        private static bool IsUppercaseError(string description)
        {
            return description.Contains("uppercase") || 
                   description.Contains("upper-case") || 
                   description.Contains("store bokstav");
        }

        /// <summary>
        /// Checks if error description indicates lowercase letter requirement
        /// </summary>
        /// <param name="description">The error description to check</param>
        /// <returns>True if error is about lowercase requirement</returns>
        private static bool IsLowercaseError(string description)
        {
            return description.Contains("lowercase") || 
                   description.Contains("lower-case") || 
                   description.Contains("liten bokstav");
        }

        /// <summary>
        /// Checks if error description indicates digit requirement
        /// </summary>
        /// <param name="description">The error description to check</param>
        /// <returns>True if error is about digit requirement</returns>
        private static bool IsDigitError(string description)
        {
            return description.Contains("digit") || 
                   description.Contains("siffer") || 
                   description.Contains("number");
        }

        /// <summary>
        /// Checks if error description indicates non-alphanumeric character requirement
        /// </summary>
        /// <param name="description">The error description to check</param>
        /// <returns>True if error is about non-alphanumeric requirement</returns>
        private static bool IsNonAlphanumericError(string description)
        {
            return description.Contains("non alphanumeric") || 
                   description.Contains("non-alphanumeric") || 
                   description.Contains("ikke-alfanumerisk");
        }

        /// <summary>
        /// Adds standard error message for invalid login
        /// </summary>
        private void AddLoginError()
        {
            ModelState.AddModelError(string.Empty, ErrorInvalidCredentials);
        }


        /// <summary>
        /// Creates full URL for password reset
        /// </summary>
        /// <param name="token">The encoded reset token</param>
        /// <param name="email">The user's email address</param>
        /// <returns>Full URL that can be sent in email</returns>
        private string CreatePasswordResetUrl(string token, string email)
        {
            return Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values: new { token, email },
                protocol: Request.Scheme)!;
        }


        /// <summary>
        /// Redirects user to the appropriate page based on their role
        /// </summary>
        /// <param name="user">The user to redirect</param>
        /// <returns>Redirect result to role-specific page</returns>
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

            // Pilots and other approved users go to Map page
            return RedirectToAction(ActionMap, ControllerMap);
        }
    }
}