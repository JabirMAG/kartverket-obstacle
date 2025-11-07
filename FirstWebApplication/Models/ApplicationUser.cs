using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Models;

public class ApplicationUser : IdentityUser
{ 
    public string? DesiredRole { get; set; }
    // dette gjør at defaulten er at den ikke er godkjent helt til den faktisk bli av Admin
    public bool IaApproved { get; set; } = false;
    public string? FullName { get; set; }
    
}