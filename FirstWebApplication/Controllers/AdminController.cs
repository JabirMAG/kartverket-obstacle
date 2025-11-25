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
    /// <summary>
    /// Controller for admin functionality. Handles user management, obstacle reports, and system administration
    /// </summary>
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
        private readonly IArchiveRepository _archiveRepository;

        private static readonly string[] AllowedAssignableRoles = new[] { "Pilot", "Registerfører" };
        private const string DefaultAdminEmail = "admin@kartverket.com";

        public AdminController(
            IUserRepository userRepository,
            IRegistrarRepository registrarRepository,
            IObstacleRepository obstacleRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDBContext context,
            IArchiveRepository archiveRepository)
        {
            _userRepository = userRepository;
            _registrarRepository = registrarRepository;
            _obstacleRepository = obstacleRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _archiveRepository = archiveRepository;
        }
        
        /// <summary>
        /// Displays the admin dashboard
        /// </summary>
        /// <returns>The admin dashboard view</returns>
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
        
        /// <summary>
        /// Displays all obstacle reports that need admin review. Filters out rejected obstacles (status 3) as they are archived
        /// </summary>
        /// <returns>The admin reports view with unique obstacles</returns>
        [HttpGet("reports")]
        public async Task<IActionResult> Reports()
        {
            // Get all obstacles that have reports
            var rapports = await _registrarRepository.GetAllRapports();
            // Filter out rejected obstacles (status 3) as they are archived
            // Group by ObstacleId and take the first report for each obstacle to avoid duplicates
            var uniqueRapports = rapports
                .Where(r => r.Obstacle != null && r.Obstacle.ObstacleStatus != 3)
                .GroupBy(r => r.ObstacleId)
                .Select(g => g.First())
                .ToList();
            
            return View("AdminViewReports", uniqueRapports);
        }
        
        /// <summary>
        /// Displays all archived reports (rejected obstacles)
        /// </summary>
        /// <returns>The archived reports view</returns>
        [HttpGet("archived-reports")]
        public async Task<IActionResult> ArchivedReports()
        {
            var archivedReports = await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
            
            return View("AdminArchivedReports", archivedReports);
        }
        
        [Authorize(Roles = "Admin")] // Kun admin får tilgang
        [HttpGet("admin/feedback")]
        public async Task<IActionResult> Feedback()
        {
            // Hent alle feedback fra databasen
            var feedbacks = await _context.Feedback
                .ToListAsync();

            // Send listen til viewet
            return View("AdminFeedback", feedbacks);
        }

        
        
        /// <summary>
        /// Restores an archived report back to active obstacles with a new status
        /// </summary>
        /// <param name="archivedReportId">The ID of the archived report to restore</param>
        /// <param name="newStatus">The new status for the restored obstacle (1 = Under behandling, 2 = Godkjent)</param>
        /// <returns>Redirects to archived reports page</returns>
        [HttpPost("restore-archived-report")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreArchivedReport(int archivedReportId, int newStatus)
        {
            // Validate that newStatus is either 1 (Under behandling) or 2 (Godkjent)
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

            // Create new ObstacleData from ArchivedReport
            var restoredObstacle = new ObstacleData
            {
                ObstacleName = archivedReport.ObstacleName,
                ObstacleHeight = archivedReport.ObstacleHeight,
                ObstacleDescription = archivedReport.ObstacleDescription,
                GeometryGeoJson = archivedReport.GeometryGeoJson,
                ObstacleStatus = newStatus,
                OwnerUserId = null // We don't have OwnerUserId in ArchivedReport, so set to null
            };

            // Put the obstacle in the database - AddObstacle returns obstacle with ObstacleId
            var savedObstacle = await _obstacleRepository.AddObstacle(restoredObstacle);

            // Parse and create RapportData entries from RapportComments
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

            // Create RapportData for each comment
            foreach (var comment in rapportComments)
            {
                var rapport = new RapportData
                {
                    ObstacleId = savedObstacle.ObstacleId,
                    RapportComment = comment
                };
                await _context.Rapports.AddAsync(rapport);
            }

            // Delete from ArchivedReport
            _context.ArchivedReports.Remove(archivedReport);

            await _context.SaveChangesAsync();

            var statusText = newStatus == 1 ? "Under behandling" : "Godkjent";
            TempData["Success"] = $"Hindring '{archivedReport.ObstacleName}' er gjenopprettet med status '{statusText}' og {rapportComments.Count} rapport(er) er lagt til.";
            
            return RedirectToAction(nameof(ArchivedReports));
        }
        
        /// <summary>
        /// Updates the status of an obstacle. If status is 3 (Rejected), the obstacle is archived
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to update</param>
        /// <param name="status">The new status for the obstacle</param>
        /// <param name="returnUrl">Optional return URL to redirect back to details page</param>
        /// <returns>Redirects to reports page or details page based on returnUrl</returns>
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
            
            // If the status is Rejected (3), archive the obstacle and all reports
            if (status == 3)
            {
                var archivedReportCount = await _archiveRepository.ArchiveObstacleAsync(obstacle);
                TempData["Success"] = $"Hindring '{obstacle.ObstacleName}' er avvist og arkivert sammen med {archivedReportCount} rapport(er).";
            }
            else
            {
                await _obstacleRepository.UpdateObstacles(obstacle);
                TempData["Success"] = $"Status for hindring '{obstacle.ObstacleName}' er oppdatert.";
            }
            
            // Redirect back to the details page if returnUrl is set, otherwise to reports
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DetaljerOmRapport"))
            {
                // If status is Rejected, redirect to reports since the obstacle is deleted
                if (status == 3)
                {
                    return RedirectToAction(nameof(Reports));
                }
                return RedirectToAction(nameof(DetaljerOmRapport), new { obstacleId });
            }
            
            return RedirectToAction(nameof(Reports));
        }
        
        /// <summary>
        /// Displays detailed view of an obstacle and all its associated reports
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle to view</param>
        /// <returns>The obstacle details view, or redirects to reports if obstacle not found</returns>
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
        
        /// <summary>
        /// Adds a comment to an obstacle's report
        /// </summary>
        /// <param name="obstacleId">The ID of the obstacle</param>
        /// <param name="comment">The comment text to add</param>
        /// <returns>Redirects to obstacle details page</returns>
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
        
        /// <summary>
        /// Displays user management page with all users and their roles
        /// </summary>
        /// <returns>The user management view</returns>
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
                    DesiredRole = u.DesiredRole,
                    Organization = u.Organization
                });
            }

            ViewBag.AssignableRoles = AllowedAssignableRoles;
            return View(vm);
        }
        
        /// <summary>
        /// Adds a role to a user. Users can only have one role at a time. Default admin account is protected from changes.
        /// </summary>
        /// <param name="model">The role assignment data</param>
        /// <returns>Redirects to user management page</returns>
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
            if (IsDefaultAdmin(user))
            {
                return RedirectToActionWithError(nameof(ManageUsers), $"Kan ikke endre standard administrator ({DefaultAdminEmail}).");
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
        
        /// <summary>
        /// Removes a role from a user. Default admin account is protected from changes.
        /// </summary>
        /// <param name="model">The role removal data</param>
        /// <returns>Redirects to user management page</returns>
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
            if (IsDefaultAdmin(user))
            {
                return RedirectToActionWithError(nameof(ManageUsers), $"Kan ikke endre standard administrator ({DefaultAdminEmail}).");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.Role);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? $"Fjernet rolle {model.Role} for {user.Email}." :
                string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }
        
        /// <summary>
        /// Deletes a user from the system. Default admin account cannot be deleted.
        /// </summary>
        /// <param name="UserId">The ID of the user to delete</param>
        /// <returns>Redirects to user management page</returns>
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
            if (IsDefaultAdmin(user))
            {
                return RedirectToActionWithError(nameof(ManageUsers), $"Kan ikke slette standard administrator ({DefaultAdminEmail}).");
            }

            var deleteResult = await _userRepository.DeleteAsync(user);
            TempData[deleteResult.Succeeded ? "Success" : "Error"] =
                deleteResult.Succeeded ? $"Bruker {user.Email} er slettet." :
                string.Join(", ", deleteResult.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }
        
        /// <summary>
        /// Approves a user account, allowing them to log in. Automatically assigns desired role if specified. Default admin account is protected from changes.
        /// </summary>
        /// <param name="UserId">The ID of the user to approve</param>
        /// <returns>Redirects to user management page</returns>
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
            if (IsDefaultAdmin(user))
            {
                return RedirectToActionWithError(nameof(ManageUsers), $"Kan ikke endre standard administrator ({DefaultAdminEmail}).");
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
        
        /// <summary>
        /// Rejects a user account, preventing them from logging in. Default admin account is protected from changes.
        /// </summary>
        /// <param name="UserId">The ID of the user to reject</param>
        /// <returns>Redirects to user management page</returns>
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
            if (IsDefaultAdmin(user))
            {
                return RedirectToActionWithError(nameof(ManageUsers), $"Kan ikke endre standard administrator ({DefaultAdminEmail}).");
            }

            user.IaApproved = false;
            var result = await _userRepository.UpdateAsync(user);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? $"Bruker {user.Email} er avvist." :
                string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(ManageUsers));
        }
        
        /// <summary>
        /// Displays the create user form
        /// </summary>
        /// <returns>The create user view</returns>
        [HttpGet("create-user")]
        public IActionResult CreateUser()
        {
            PopulateCreateUserViewData();
            return View(new CreateUserVm());
        }
        
        /// <summary>
        /// Creates a new user account with the specified role. Admin-created users are automatically approved
        /// </summary>
        /// <param name="model">The user creation data</param>
        /// <returns>Redirects to user management page on success, or returns create user view with errors</returns>
        [ValidateAntiForgeryToken]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserVm model)
        {
            if (!OrganizationOptions.All.Contains(model.Organization ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Organization), "Velg en gyldig organisasjon.");
            }
            else
            {
                model.Organization = OrganizationOptions.All.First(o =>
                    string.Equals(o, model.Organization, StringComparison.OrdinalIgnoreCase));
            }

            if (!ModelState.IsValid)
            {
                PopulateCreateUserViewData();
                return View(model);
            }

            var existing = await _userRepository.GetByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "E-posten er allerede i bruk.");
                PopulateCreateUserViewData();
                return View(model);
            }

            var existingUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUsername != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Brukernavnet er allerede i bruk.");
                PopulateCreateUserViewData();
                return View(model);
            }

            if (!AllowedAssignableRoles.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Ugyldig rolle.");
                PopulateCreateUserViewData();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                IaApproved = true, // Admin-created users are automatically approved
                Organization = model.Organization
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
                    // Message
                    ModelState.AddModelError(nameof(model.Password), "Passordet oppfyller ikke kravene. Følg kravene som vises under passordfeltet.");

                    // Mssages near the password field so non-programmers understand
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

                PopulateCreateUserViewData();
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] = $"Opprettet bruker {model.Email} med rolle {model.Role}.";
            return RedirectToAction(nameof(ManageUsers));
        }

        private void PopulateCreateUserViewData()
        {
            ViewBag.AssignableRoles = AllowedAssignableRoles;
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

        /// <summary>
        /// Helper method to check if user is the default admin account
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <returns>True if the user is the default admin account, false otherwise</returns>
        private bool IsDefaultAdmin(ApplicationUser? user)
        {
            return user != null && 
                   !string.IsNullOrEmpty(user.Email) &&
                   string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Helper method to redirect with error message
        /// </summary>
        /// <param name="actionName">The action name to redirect to</param>
        /// <param name="errorMessage">The error message to display</param>
        /// <returns>Redirect result to the specified action</returns>
        private IActionResult RedirectToActionWithError(string actionName, string errorMessage)
        {
            TempData["Error"] = errorMessage;
            return RedirectToAction(actionName);
        }
    }
}
