using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.ViewModel;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Brukernavn er påkrevd")]
    [StringLength(50, ErrorMessage = "Brukernavn kan ikke være lengre enn 50 tegn")]
    [Display(Name = "Brukernavn")]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    [Display(Name = "Email")] 
    public string Email { get; set; }
   
    [Required(ErrorMessage = "Rolle er påkrevd")]
    [Display(Name = "Ønsket rolle")]
    public string DesiredRole { get; set; } 
    
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    
}