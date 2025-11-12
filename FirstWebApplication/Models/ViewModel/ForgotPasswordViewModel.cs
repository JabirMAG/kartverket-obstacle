using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-post er påkrevd")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
    [Display(Name = "E-post")]
    public string Email { get; set; }
}

