using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for user data operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Returns queryable users for listing and filtering
        /// </summary>
        IQueryable<ApplicationUser> Query();
        
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        Task<ApplicationUser?> GetByIdAsync(string id);
        
        /// <summary>
        /// Gets a user by email address
        /// </summary>
        Task<ApplicationUser?> GetByEmailAsync(string email);
        
        /// <summary>
        /// Gets a user by username
        /// </summary>
        Task<ApplicationUser?> GetByNameAsync(string username);
        
        /// <summary>
        /// Gets the user associated with the specified claims principal
        /// </summary>
        Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal);
        
        /// <summary>
        /// Creates a new user with password
        /// </summary>
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        
        /// <summary>
        /// Deletes a user
        /// </summary>
        Task<IdentityResult> DeleteAsync(ApplicationUser user);
        
        /// <summary>
        /// Validates and normalizes organization name (case-insensitive matching)
        /// </summary>
        /// <param name="organization">The organization name to validate</param>
        /// <returns>Normalized organization name or null if invalid</returns>
        string? ValidateAndNormalizeOrganization(string? organization);
        
        /// <summary>
        /// Checks if an organization is valid
        /// </summary>
        /// <param name="organization">The organization name to check</param>
        /// <returns>True if organization is valid, false otherwise</returns>
        bool IsValidOrganization(string? organization);
        
        /// <summary>
        /// Gets all valid organization options
        /// </summary>
        /// <returns>Array of all valid organization names</returns>
        string[] GetAllOrganizations();
        
        /// <summary>
        /// Generates and encodes password reset token to Base64Url format
        /// </summary>
        /// <param name="user">The user who should receive the reset token</param>
        /// <returns>Encoded token that can be used in URL</returns>
        Task<string> GenerateEncodedPasswordResetTokenAsync(ApplicationUser user);
        
        /// <summary>
        /// Resets password for a user using the provided token
        /// </summary>
        /// <param name="user">The user whose password should be reset</param>
        /// <param name="encodedToken">The encoded reset token from URL</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>Identity result indicating success or failure</returns>
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string encodedToken, string newPassword);
        
        /// <summary>
        /// Checks if a user is in a specific role
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <param name="role">The role name to check</param>
        /// <returns>True if user is in the role, false otherwise</returns>
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
        
        /// <summary>
        /// Gets password options for validation and display
        /// </summary>
        /// <returns>Password options configuration</returns>
        PasswordOptions GetPasswordOptions();
        
        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        /// <param name="user">The user to get roles for</param>
        /// <returns>List of role names</returns>
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        
        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="user">The user to add to the role</param>
        /// <param name="role">The role name</param>
        /// <returns>Identity result indicating success or failure</returns>
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        
        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="user">The user to remove from the role</param>
        /// <param name="role">The role name</param>
        /// <returns>Identity result indicating success or failure</returns>
        Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role);
        
        /// <summary>
        /// Checks if a role exists
        /// </summary>
        /// <param name="role">The role name to check</param>
        /// <returns>True if role exists, false otherwise</returns>
        Task<bool> RoleExistsAsync(string role);
        
        /// <summary>
        /// Ensures a role exists, creating it if it doesn't
        /// </summary>
        /// <param name="role">The role name to ensure exists</param>
        /// <returns>Task representing the async operation</returns>
        Task EnsureRoleExistsAsync(string role);
    }
}
