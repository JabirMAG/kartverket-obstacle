using Microsoft.AspNetCore.Identity;

namespace FirstWebApplication.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    
}