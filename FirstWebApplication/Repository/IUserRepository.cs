using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;

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
    }
}
