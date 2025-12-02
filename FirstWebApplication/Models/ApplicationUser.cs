using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Models;

/// <summary>
/// Extended user model for the application. Default approval status is false until approved by Admin
/// </summary>
public class ApplicationUser : IdentityUser
{ 
    public string? DesiredRole { get; set; }
    public bool IaApproved { get; set; } = false;
    public string? FullName { get; set; }
    public new string? Email { get; set; }
    public string? Organization { get; set; }
}