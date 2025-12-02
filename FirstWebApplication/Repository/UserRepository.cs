using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for user data operations
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Returns queryable users for listing and filtering
        /// </summary>
        public IQueryable<ApplicationUser> Query() => _userManager.Users;

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        public Task<ApplicationUser?> GetByIdAsync(string id) => _userManager.FindByIdAsync(id);

        /// <summary>
        /// Gets a user by email address
        /// </summary>
        public Task<ApplicationUser?> GetByEmailAsync(string email) => _userManager.FindByEmailAsync(email);

        /// <summary>
        /// Gets a user by username
        /// </summary>
        public Task<ApplicationUser?> GetByNameAsync(string username) => _userManager.FindByNameAsync(username);

        /// <summary>
        /// Gets the user associated with the specified claims principal
        /// </summary>
        public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) => _userManager.GetUserAsync(principal);

        /// <summary>
        /// Creates a new user with password
        /// </summary>
        public Task<IdentityResult> CreateAsync(ApplicationUser user, string password) => _userManager.CreateAsync(user, password);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        public Task<IdentityResult> UpdateAsync(ApplicationUser user) => _userManager.UpdateAsync(user);

        /// <summary>
        /// Deletes a user
        /// </summary>
        public async Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            return await _userManager.DeleteAsync(user);
        }

        /// <summary>
        /// Validates and normalizes organization name (case-insensitive matching)
        /// </summary>
        /// <param name="organization">The organization name to validate</param>
        /// <returns>Normalized organization name or null if invalid</returns>
        public string? ValidateAndNormalizeOrganization(string? organization)
        {
            if (!IsValidOrganization(organization))
            {
                return null;
            }
            
            return OrganizationOptions.All.First(o =>
                string.Equals(o, organization, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if an organization is valid
        /// </summary>
        /// <param name="organization">The organization name to check</param>
        /// <returns>True if organization is valid, false otherwise</returns>
        public bool IsValidOrganization(string? organization)
        {
            return OrganizationOptions.All.Contains(organization ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets all valid organization options
        /// </summary>
        /// <returns>Array of all valid organization names</returns>
        public string[] GetAllOrganizations() => OrganizationOptions.All;

        /// <summary>
        /// Generates and encodes password reset token to Base64Url format
        /// </summary>
        /// <param name="user">The user who should receive the reset token</param>
        /// <returns>Encoded token that can be used in URL</returns>
        public async Task<string> GenerateEncodedPasswordResetTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        /// <summary>
        /// Resets password for a user using the provided token
        /// </summary>
        /// <param name="user">The user whose password should be reset</param>
        /// <param name="encodedToken">The encoded reset token from URL</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>Identity result indicating success or failure</returns>
        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string encodedToken, string newPassword)
        {
            var decodedToken = DecodePasswordResetToken(encodedToken);
            return await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        }

        /// <summary>
        /// Checks if a user is in a specific role
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <param name="role">The role name to check</param>
        /// <returns>True if user is in the role, false otherwise</returns>
        public Task<bool> IsInRoleAsync(ApplicationUser user, string role) => _userManager.IsInRoleAsync(user, role);

        /// <summary>
        /// Gets password options for validation and display
        /// </summary>
        /// <returns>Password options configuration</returns>
        public PasswordOptions GetPasswordOptions() => _userManager.Options.Password;

        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        /// <param name="user">The user to get roles for</param>
        /// <returns>List of role names</returns>
        public Task<IList<string>> GetRolesAsync(ApplicationUser user) => _userManager.GetRolesAsync(user);

        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="user">The user to add to the role</param>
        /// <param name="role">The role name</param>
        /// <returns>Identity result indicating success or failure</returns>
        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role) => _userManager.AddToRoleAsync(user, role);

        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="user">The user to remove from the role</param>
        /// <param name="role">The role name</param>
        /// <returns>Identity result indicating success or failure</returns>
        public Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role) => _userManager.RemoveFromRoleAsync(user, role);

        /// <summary>
        /// Checks if a role exists
        /// </summary>
        /// <param name="role">The role name to check</param>
        /// <returns>True if role exists, false otherwise</returns>
        public Task<bool> RoleExistsAsync(string role) => _roleManager.RoleExistsAsync(role);

        /// <summary>
        /// Ensures a role exists, creating it if it doesn't
        /// </summary>
        /// <param name="role">The role name to ensure exists</param>
        /// <returns>Task representing the async operation</returns>
        public async Task EnsureRoleExistsAsync(string role)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                return;
            }

            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        /// <summary>
        /// Decodes password reset token from Base64Url format
        /// </summary>
        /// <param name="encodedToken">The encoded token from URL</param>
        /// <returns>Decoded token that can be used in ResetPasswordAsync</returns>
        private static string DecodePasswordResetToken(string encodedToken)
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
            return Encoding.UTF8.GetString(tokenBytes);
        }
    }
}
