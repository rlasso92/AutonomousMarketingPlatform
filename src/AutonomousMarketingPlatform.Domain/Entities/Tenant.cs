using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string SchemaName { get; set; } = null!;
        public bool IsDisabled { get; set; }
    }
}
