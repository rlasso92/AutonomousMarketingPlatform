using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    // Add additional properties here if needed
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsDisabled { get; set; }

    // Profile Information
    public string? Bio { get; set; }
    public string? Organization { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? SocialMediaLinks { get; set; } // Can be a JSON string
    public string? AvatarBase64 { get; set; }

    // MFA
    public string? TwoFactorSecretKey { get; set; }
}
