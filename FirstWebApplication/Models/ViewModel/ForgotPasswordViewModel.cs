using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel;

// ViewModel for glemt passord-skjema
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-post er påkrevd")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
    [Display(Name = "E-post")]
    public string Email { get; set; }
}
