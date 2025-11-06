using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Repository
{
    public interface IUserRepository
    {
        IQueryable<ApplicationUser> Query();          // til liste/filtrering
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<IdentityResult> DeleteAsync(ApplicationUser user);
    }
}
