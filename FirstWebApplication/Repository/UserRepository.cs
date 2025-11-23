using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for user data operations
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
    }
}
