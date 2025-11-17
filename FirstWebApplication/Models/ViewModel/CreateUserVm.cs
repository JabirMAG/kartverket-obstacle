using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.AdminViewModels
{
    public class CreateUserVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Display(Name = "Rolle")]
        public string Role { get; set; } = "Pilot"; // standardvalg
    }
}
