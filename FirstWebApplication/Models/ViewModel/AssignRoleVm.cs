using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.AdminViewModels
{
    // ViewModel for tildeling eller fjerning av en rolle fra en bruker
    public class AssignRoleVm
    {
        [Required]
        public string UserId { get; set; } = default!;
        [Required]
        public string Role { get; set; } = default!;
    }
}
