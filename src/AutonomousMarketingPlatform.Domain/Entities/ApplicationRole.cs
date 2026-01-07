using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
