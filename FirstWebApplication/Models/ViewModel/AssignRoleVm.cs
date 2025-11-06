using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.AdminViewModels
{
    public class AssignRoleVm
    {
        [Required]
        public string UserId { get; set; } = default!;
        [Required]
        public string Role { get; set; } = default!;
    }
}
