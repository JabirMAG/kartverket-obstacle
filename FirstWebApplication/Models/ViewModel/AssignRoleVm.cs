using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.AdminViewModels
{
    /// <summary>
    /// ViewModel for assigning or removing a role from a user
    /// </summary>
    public class AssignRoleVm
    {
        [Required]
        public string UserId { get; set; } = default!;
        [Required]
        public string Role { get; set; } = default!;
    }
}
