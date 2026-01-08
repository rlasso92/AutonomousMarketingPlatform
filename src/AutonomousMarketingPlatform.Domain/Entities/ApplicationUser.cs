using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    // Add additional properties here if needed
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsDisabled { get; set; }
}
