using FirstWebApplication.Models;
using FirstWebApplication.Models.AdminViewModels;
using FirstWebApplication.Repositories;
using FirstWebApplication.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FirstWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRegistrarRepository _registrarRepository;
        private readonly IObstacleRepository _obstacleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDBContext _context;

        private static readonly string[] AllowedAssignableRoles = new[] { "Pilot", "Registerfører" };
        private const string DefaultAdminEmail = "admin@kartverket.com";

        public AdminController(
            IUserRepository userRepository,
            IRegistrarRepository registrarRepository,
            IObstacleRepository obstacleRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDBContext context)
        {
            _userRepository = userRepository;
            _registrarRepository = registrarRepository;
            _obstacleRepository = obstacleRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("reports")]
        public async Task<IActionResult> Reports()
        {
            // Hent alle hindringer som har rapporter
            var rapports = await _registrarRepository.GetAllRapports();
            // Filtrer ut rejected hindringer (status 3) siden de er arkivert
            // Grupper på ObstacleId og ta den første rapporten for hver hindring for å unngå duplikater
            var uniqueRapports = rapports
                .Where(r => r.Obstacle != null && r.Obstacle.ObstacleStatus != 3)
                .GroupBy(r => r.ObstacleId)
                .Select(g => g.First())
                .ToList();
            
            return View("AdminViewReports", uniqueRapports);
        }

        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
            
            return View("AdminArchivedReports", archivedReports);
        }

        [HttpPost("restore-archived-report")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreArchivedReport(int archivedReportId, int newStatus)
        {
            // Valider at newStatus er enten 1 (Under behandling) eller 2 (Godkjent)
            if (newStatus != 1 && newStatus != 2)
            {
                TempData["Error"] = "Ugyldig status. Status må være 'Under behandling' (1) eller 'Godkjent' (2).";
                return RedirectToAction(nameof(ArchivedReports));
            }

            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.ArchivedReportId == archivedReportId);

            if (archivedReport == null)
            {
                TempData["Error"] = "Fant ikke arkivert rapport.";
                return RedirectToAction(nameof(ArchivedReports));
            }

            // Opprett ny ObstacleData fra ArchivedReport
            var restoredObstacle = new ObstacleData
            {
                ObstacleName = archivedReport.ObstacleName,
                ObstacleHeight = archivedReport.ObstacleHeight,
                ObstacleDescription = archivedReport.ObstacleDescription,
                GeometryGeoJson = archivedReport.GeometryGeoJson,
                ObstacleStatus = newStatus,
                OwnerUserId = null // Vi har ikke OwnerUserId i ArchivedReport, så setter til null
            };

            // Legg til hindringen i databasen (AddObstacle returnerer obstacle med ObstacleId satt)
            var savedObstacle = await _obstacleRepository.AddObstacle(restoredObstacle);

            // Deserialiser og opprett RapportData-entries fra RapportComments
            List<string> rapportComments = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(archivedReport.RapportComments))
                {
                    rapportComments = JsonSerializer.Deserialize<List<string>>(archivedReport.RapportComments) ?? new List<string>();
                }
            }
            catch
            {
                rapportComments = new List<string>();
            }

            // Opprett RapportData for hver kommentar
            foreach (var comment in rapportComments)
            {
                var rapport = new RapportData
                {
                    ObstacleId = savedObstacle.ObstacleId,
                    RapportComment = comment
                };
                await _context.Rapports.AddAsync(rapport);
            }

            // Slett ArchivedReport
            _context.ArchivedReports.Remove(archivedReport);

            await _context.SaveChangesAsync();

            var statusText = newStatus == 1 ? "Under behandling" : "Godkjent";
            TempData["Success"] = $"Hindring '{archivedReport.ObstacleName}' er gjenopprettet med status '{statusText}' og {rapportComments.Count} rapport(er) er lagt til.";
            
            return RedirectToAction(nameof(ArchivedReports));
        }

        [HttpPost("update-obstacle-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateObstacleStatus(int obstacleId, int status, string returnUrl = null)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Reports));
            }

            obstacle.ObstacleStatus = status;
            
            // Hvis status er Rejected (3), arkiver hindringen og alle rapporter
            if (status == 3)
            {
                // Hent alle rapporter knyttet til hindringen
                var rapports = await _context.Rapports
                    .Where(r => r.ObstacleId == obstacle.ObstacleId)
                    .ToListAsync();
                
                // Kombiner alle rapport-kommentarer til en JSON array
                var rapportComments = rapports.Select(r => r.RapportComment).ToList();
                var rapportCommentsJson = JsonSerializer.Serialize(rapportComments);
                
                var archivedReport = new ArchivedReport
                {
                    OriginalObstacleId = obstacle.ObstacleId,
                    ObstacleName = obstacle.ObstacleName,
                    ObstacleHeight = obstacle.ObstacleHeight,
                    ObstacleDescription = obstacle.ObstacleDescription,
                    GeometryGeoJson = obstacle.GeometryGeoJson,
                    ObstacleStatus = 3,
                    ArchivedDate = DateTime.UtcNow,
                    RapportComments = rapportCommentsJson
                };
                
                await _context.ArchivedReports.AddAsync(archivedReport);
                
                // Slett rapporter fra Rapports-tabellen (de er nå lagret i ArchivedReport)
                if (rapports.Any())
                {
                    _context.Rapports.RemoveRange(rapports);
                }
                
                await _context.SaveChangesAsync();
                
                // Slett hindringen fra ObstaclesData (som UpdateObstacles gjør når status er 3)
                await _obstacleRepository.UpdateObstacles(obstacle);
                
                TempData["Success"] = $"Hindring '{obstacle.ObstacleName}' er avvist og arkivert sammen med {rapports.Count} rapport(er).";
            }
            else
            {
                await _obstacleRepository.UpdateObstacles(obstacle);
                TempData["Success"] = $"Status for hindring '{obstacle.ObstacleName}' er oppdatert.";
            }
            
            // Redirect tilbake til detaljsiden hvis returnUrl er satt, ellers til rapporter
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
                // Hvis status er Rejected, redirect til rapporter siden hindringen er slettet
                if (status == 3)
                {
                    return RedirectToAction(nameof(Reports));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }
            
            return RedirectToAction(nameof(Reports));
        }

        [HttpGet("report-details/{obstacleId}")]
        public async Task<IActionResult> DetaljerOmRapport(int obstacleId)
        {
            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Reports));
            }

            var rapports = await _registrarRepository.GetAllRapports();
            var obstacleRapports = rapports.Where(r => r.ObstacleId == obstacleId).ToList();

            ViewBag.Obstacle = obstacle;
            ViewBag.Rapports = obstacleRapports;

            return View("DetaljerOmRapport", obstacle);
        }

        [HttpPost("add-comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int obstacleId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Kommentar kan ikke være tom.";
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }

            var obstacle = await _obstacleRepository.GetElementById(obstacleId);
            if (obstacle == null)
            {
                TempData["Error"] = "Fant ikke hindring.";
                return RedirectToAction(nameof(Reports));
            }

            var rapport = new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = comment
            };

            await _registrarRepository.AddRapport(rapport);
            TempData["Success"] = "Kommentar lagt til.";
            return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
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
                    Roles = roles,
                    IsApproved = u.IaApproved,
                    DesiredRole = u.DesiredRole
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

        [ValidateAntiForgeryToken]
        [HttpPost("approve-user")]
        public async Task<IActionResult> ApproveUser(string UserId)
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

            // Protect default admin account from changes
            if (!string.IsNullOrEmpty(user.Email) &&
                string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Kan ikke endre standard administrator ({DefaultAdminEmail}).";
                return RedirectToAction(nameof(ManageUsers));
            }

            user.IaApproved = true;
            var result = await _userRepository.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                // Reload user from database to ensure we have the latest data
                user = await _userRepository.GetByIdAsync(UserId);
                if (user == null)
                {
                    TempData["Error"] = "Fant ikke bruker etter oppdatering.";
                    return RedirectToAction(nameof(ManageUsers));
                }
                
                // Assign the DesiredRole if the user has one and doesn't already have a role
                if (!string.IsNullOrEmpty(user.DesiredRole))
                {
                    var existingRoles = await _userManager.GetRolesAsync(user);
                    if (!existingRoles.Any())
                    {
                        // Check if the desired role is allowed
                        if (AllowedAssignableRoles.Contains(user.DesiredRole))
                        {
                            // Ensure the role exists
                            if (!await _roleManager.RoleExistsAsync(user.DesiredRole))
                            {
                                await _roleManager.CreateAsync(new IdentityRole(user.DesiredRole));
                            }
                            
                            // Assign the role
                            var roleResult = await _userManager.AddToRoleAsync(user, user.DesiredRole);
                            if (!roleResult.Succeeded)
                            {
                                TempData["Error"] = $"Bruker {user.Email} er godkjent, men kunne ikke tildele rolle: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}.";
                                return RedirectToAction(nameof(ManageUsers));
                            }
                        }
                    }
                }
                
                TempData["Success"] = $"Bruker {user.Email} er godkjent{(string.IsNullOrEmpty(user.DesiredRole) ? "" : $" og tildelt rolle {user.DesiredRole}")}.";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("reject-user")]
        public async Task<IActionResult> RejectUser(string UserId)
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

            // Protect default admin account from changes
            if (!string.IsNullOrEmpty(user.Email) &&
                string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"Kan ikke endre standard administrator ({DefaultAdminEmail}).";
                return RedirectToAction(nameof(ManageUsers));
            }

            user.IaApproved = false;
            var result = await _userRepository.UpdateAsync(user);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? $"Bruker {user.Email} er avvist." :
                string.Join(", ", result.Errors.Select(e => e.Description));

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
                Email = model.Email,
                IaApproved = true // Admin-created users are automatically approved
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
