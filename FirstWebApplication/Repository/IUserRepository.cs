using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FirstWebApplication.Repositories
{
    // Repository-grensesnitt for brukerdataoperasjoner
    public interface IUserRepository
    {
        // Returnerer spørbare brukere for listing og filtrering
        IQueryable<ApplicationUser> Query();
        
        // Henter en bruker etter ID
        Task<ApplicationUser?> GetByIdAsync(string id);
        
        // Henter en bruker etter e-postadresse
        Task<ApplicationUser?> GetByEmailAsync(string email);
        
        // Henter en bruker etter brukernavn
        Task<ApplicationUser?> GetByNameAsync(string username);
        
        // Henter brukeren tilknyttet den spesifiserte claims principal
        Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal);
        
        // Oppretter en ny bruker med passord
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        
        // Oppdaterer en eksisterende bruker
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        
        // Sletter en bruker
        Task<IdentityResult> DeleteAsync(ApplicationUser user);
        
        // Validerer og normaliserer organisasjonsnavn (case-insensitive matching)
        string? ValidateAndNormalizeOrganization(string? organization);
        
        // Sjekker om en organisasjon er gyldig
        bool IsValidOrganization(string? organization);
        
        // Henter alle gyldige organisasjonsalternativer
        string[] GetAllOrganizations();
        
        // Genererer og koder passordtilbakestillingsnøkkel til Base64Url-format
        Task<string> GenerateEncodedPasswordResetTokenAsync(ApplicationUser user);
        
        // Tilbakestiller passord for en bruker ved hjelp av den oppgitte nøkkelen
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string encodedToken, string newPassword);
        
        // Sjekker om en bruker er i en spesifikk rolle
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        
        // Henter passordalternativer for validering og visning
        PasswordOptions GetPasswordOptions();
        
        // Henter alle roller for en bruker
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        
        // Legger til en bruker i en rolle
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        
        // Fjerner en bruker fra en rolle
        Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
        
        // Sjekker om en rolle eksisterer
        Task<bool> RoleExistsAsync(string role);
        
        // Sikrer at en rolle eksisterer, oppretter den hvis den ikke gjør det
        Task EnsureRoleExistsAsync(string role);
    }
}
