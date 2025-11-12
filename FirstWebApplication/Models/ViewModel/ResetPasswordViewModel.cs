using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Token { get; set; }
    
    [Required(ErrorMessage = "Passord er påkrevd")]
    [StringLength(100, ErrorMessage = "Passordet må være minst {2} tegn langt.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nytt passord")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Bekreft passord er påkrevd")]
    [DataType(DataType.Password)]
    [Display(Name = "Bekreft passord")]
    [Compare("Password", ErrorMessage = "Passordene stemmer ikke overens.")]
    public string ConfirmPassword { get; set; }
}

