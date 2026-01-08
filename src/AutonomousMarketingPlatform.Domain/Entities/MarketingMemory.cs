using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    /// <summary>
    /// Represents a key-value store for the AI to remember specific facts, 
    /// preferences, or outcomes from marketing activities.
    /// </summary>
    public class MarketingMemory : BaseEntity
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}
