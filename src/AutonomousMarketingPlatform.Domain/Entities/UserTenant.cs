using AutonomousMarketingPlatform.Domain.Entities;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class UserTenant
    {
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
    }
}
