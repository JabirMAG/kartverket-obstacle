using System.ComponentModel.DataAnnotations;
using FirstWebApplication.Constants;

namespace FirstWebApplication.Models.AdminViewModels
{
    public class CreateUserVm
    {
        [Required]
        [StringLength(50, ErrorMessage = "Brukernavn kan ikke være lengre enn 50 tegn")]
        [Display(Name = "Brukernavn")]
        public string Username { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Display(Name = "Rolle")]
        public string Role { get; set; } = "Pilot"; // standardvalg

        [Required(ErrorMessage = "Organisasjon er påkrevd")]
        [Display(Name = "Organisasjon")]
        public string Organization { get; set; } = OrganizationOptions.Kartverket;
    }
}
