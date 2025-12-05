using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace FirstWebApplication.Repositories
{
    // Repository for brukerdataoperasjoner
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Returnerer spørbare brukere for listing og filtrering
        public IQueryable<ApplicationUser> Query() => _userManager.Users;

        // Henter en bruker etter ID
        public Task<ApplicationUser?> GetByIdAsync(string id) => _userManager.FindByIdAsync(id);

        // Henter en bruker etter e-postadresse
        public Task<ApplicationUser?> GetByEmailAsync(string email) => _userManager.FindByEmailAsync(email);

        // Henter en bruker etter brukernavn
        public Task<ApplicationUser?> GetByNameAsync(string username) => _userManager.FindByNameAsync(username);

        // Henter brukeren tilknyttet den spesifiserte claims principal
        public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) => _userManager.GetUserAsync(principal);

        // Oppretter en ny bruker med passord
        public Task<IdentityResult> CreateAsync(ApplicationUser user, string password) => _userManager.CreateAsync(user, password);

        // Oppdaterer en eksisterende bruker
        public Task<IdentityResult> UpdateAsync(ApplicationUser user) => _userManager.UpdateAsync(user);

        // Sletter en bruker
        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            return await _userManager.DeleteAsync(user);
        }

        // Validerer og normaliserer organisasjonsnavn (case-insensitive matching)
        public string? ValidateAndNormalizeOrganization(string? organization)
        {
            if (!IsValidOrganization(organization))
            {
                return null;
            }
            
            return OrganizationOptions.All.First(o =>
                string.Equals(o, organization, StringComparison.OrdinalIgnoreCase));
        }

        // Sjekker om en organisasjon er gyldig
        public bool IsValidOrganization(string? organization)
        {
            return OrganizationOptions.All.Contains(organization ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        }

        // Henter alle gyldige organisasjonsalternativer
        public string[] GetAllOrganizations() => OrganizationOptions.All;

        // Genererer og koder passordtilbakestillingsnøkkel til Base64Url-format
        public async Task<string> GenerateEncodedPasswordResetTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        // Tilbakestiller passord for en bruker ved hjelp av den oppgitte nøkkelen
        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string encodedToken, string newPassword)
        {
            var decodedToken = DecodePasswordResetToken(encodedToken);
            return await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        }

        // Sjekker om en bruker er i en spesifikk rolle
        public Task<bool> IsInRoleAsync(ApplicationUser user, string role) => _userManager.IsInRoleAsync(user, role);

        // Henter passordalternativer for validering og visning
        public PasswordOptions GetPasswordOptions() => _userManager.Options.Password;

        // Henter alle roller for en bruker
        public Task<IList<string>> GetRolesAsync(ApplicationUser user) => _userManager.GetRolesAsync(user);

        // Legger til en bruker i en rolle
        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role) => _userManager.AddToRoleAsync(user, role);

        // Fjerner en bruker fra en rolle
        public Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role) => _userManager.RemoveFromRoleAsync(user, role);

        // Sjekker om en rolle eksisterer
        public Task<bool> RoleExistsAsync(string role) => _roleManager.RoleExistsAsync(role);

        // Sikrer at en rolle eksisterer, oppretter den hvis den ikke gjør det
        public async Task EnsureRoleExistsAsync(string role)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                return;
            }

            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // Dekoder passordtilbakestillingsnøkkel fra Base64Url-format
        private static string DecodePasswordResetToken(string encodedToken)
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
            return Encoding.UTF8.GetString(tokenBytes);
        }
    }
}
