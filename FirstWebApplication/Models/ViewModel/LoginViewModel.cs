using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for user login form
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Brukernavn er påkrevd")]
        [Display(Name = "Brukernavn")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passord er påkrevd")]
        [DataType(DataType.Password)]
        [Display(Name = "Passord")]
        public string Password { get; set; } = string.Empty;
    }
}
