using FirstWebApplication.Models;
using FirstWebApplication.Models.AdminViewModels;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private static readonly string[] AllowedAssignableRoles = new[] { "Pilot", "Registerfører" };
        private const string DefaultAdminEmail = "admin@kartverket.com";

        public AdminController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("manage-users")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userRepository.Query().OrderBy(u => u.Email).ToListAsync();

            var vm = new List<UserWithRolesVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                vm.Add(new UserWithRolesVm
                {
                    Id = u.Id,
                    Email = u.Email!,
                    DisplayName = u.UserName,
                    Roles = roles
                });
            }

            ViewBag.AssignableRoles = AllowedAssignableRoles;
            return View(vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole(AssignRoleVm model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Ugyldig forespørsel.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userRepository.GetByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Fant ikke bruker.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Protect default admin account from any changes
            if (!string.IsNullOrEmpty(user.Email) &&
                string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Kan ikke endre standard administrator ({DefaultAdminEmail}).";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Enforce single-role rule: user must have no roles before assigning a new one
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                TempData["Error"] = "Brukeren har allerede en rolle. Fjern eksisterende rolle før du legger til en ny.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (!AllowedAssignableRoles.Contains(model.Role) && model.Role != "Admin")
            {
                TempData["Error"] = "Ugyldig rolle.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? $"La til rolle {model.Role} for {user.Email}." :
                string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole(AssignRoleVm model)
        {
            var user = await _userRepository.GetByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "Fant ikke bruker.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Protect default admin account from any changes
            if (!string.IsNullOrEmpty(user.Email) &&
                string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Kan ikke endre standard administrator ({DefaultAdminEmail}).";
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? $"Fjernet rolle {model.Role} for {user.Email}." :
                string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                TempData["Error"] = "Ugyldig forespørsel.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userRepository.GetByIdAsync(UserId);
            if (user == null)
            {
                TempData["Error"] = "Fant ikke bruker.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Protect default admin account from deletion
            if (!string.IsNullOrEmpty(user.Email) &&
                string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Kan ikke slette standard administrator ({DefaultAdminEmail}).";
                return RedirectToAction(nameof(ManageUsers));
            }

            var deleteResult = await _userRepository.DeleteAsync(user);
            TempData[deleteResult.Succeeded ? "Success" : "Error"] =
                deleteResult.Succeeded ? $"Bruker {user.Email} er slettet." :
                string.Join(", ", deleteResult.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet("create-user")]
        public IActionResult CreateUser()
        {
            ViewBag.AssignableRoles = AllowedAssignableRoles;
            var policy = BuildPasswordPolicyObject();
            ViewBag.PasswordRequirements = BuildPasswordRequirements();
            ViewBag.PasswordPolicy = policy;
            return View(new CreateUserVm());
        }

        [ValidateAntiForgeryToken]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserVm model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AssignableRoles = AllowedAssignableRoles;
                ViewBag.PasswordRequirements = BuildPasswordRequirements();
                ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
                return View(model);
            }

            var existing = await _userRepository.GetByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "E-posten er allerede i bruk.");
                ViewBag.AssignableRoles = AllowedAssignableRoles;
                ViewBag.PasswordRequirements = BuildPasswordRequirements();
                ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
                return View(model);
            }

            if (!AllowedAssignableRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Ugyldig rolle.");
                ViewBag.AssignableRoles = AllowedAssignableRoles;
                ViewBag.PasswordRequirements = BuildPasswordRequirements();
                ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var createResult = await _userRepository.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                // Detect password-related errors so they can be shown near the password field
                var pwErrors = createResult.Errors.Where(e =>
                    (!string.IsNullOrEmpty(e.Code) && e.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(e.Description) && e.Description.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

                if (pwErrors.Any())
                {
                    // Friendly, non-technical message in Norwegian
                    ModelState.AddModelError(nameof(model.Password), "Passordet oppfyller ikke kravene. Følg kravene som vises under passordfeltet.");

                    // Add translated/simplified messages near the password field so non-programmers understand
                    foreach (var pe in pwErrors)
                    {
                        var friendly = TranslatePasswordError(pe.Description, _userManager.Options.Password);
                        ModelState.AddModelError(nameof(model.Password), friendly);
                    }
                }

                // Keep other identity errors as form-level messages (original descriptions for debugging/consumers)
                var otherErrors = createResult.Errors.Except(pwErrors).ToList();
                foreach (var err in otherErrors)
                    ModelState.AddModelError(string.Empty, err.Description);

                ViewBag.AssignableRoles = AllowedAssignableRoles;
                ViewBag.PasswordRequirements = BuildPasswordRequirements();
                ViewBag.PasswordPolicy = BuildPasswordPolicyObject();
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"Opprettet bruker {model.Email} med rolle {model.Role}.";
            return RedirectToAction(nameof(ManageUsers));
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
                // extract number if present
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

            // fallback: return original (safe) but prefixed for clarity
            return "Passordfeil: " + description;
        }
    }
}
