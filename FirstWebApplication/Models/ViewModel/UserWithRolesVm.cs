using FirstWebApplication.Models;

namespace FirstWebApplication.Models.AdminViewModels
{
    /// <summary>
    /// ViewModel for displaying user information with their assigned roles
    /// </summary>
    public class UserWithRolesVm
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? DisplayName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsApproved { get; set; }
        public string? DesiredRole { get; set; }
        public string? Organization { get; set; }
    }
}
