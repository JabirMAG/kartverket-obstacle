using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.AdminViewModels
{
    /// <summary>
    /// ViewModel for creating a new user account by admin
    /// </summary>
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

        /// <summary>
        /// Default role selection
        /// </summary>
        [Display(Name = "Rolle")]
        public string Role { get; set; } = "Pilot";

        [Required(ErrorMessage = "Organisasjon er påkrevd")]
        [Display(Name = "Organisasjon")]
        public string Organization { get; set; } = Models.OrganizationOptions.Kartverket;
    }
}
