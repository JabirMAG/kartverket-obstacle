using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Models;

// Utvidet brukermodell for applikasjonen. Standard godkjenningsstatus er false til godkjent av Admin
public class ApplicationUser : IdentityUser
{ 
    public string? DesiredRole { get; set; }
    public bool IaApproved { get; set; } = false;
    public string? FullName { get; set; }
    public new string? Email { get; set; }
    public string? Organization { get; set; }
}
