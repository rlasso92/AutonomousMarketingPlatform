using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    /// <summary>
    /// Represents a collection of related marketing content and assets, 
    /// ready to be used in a campaign or a publish job.
    /// </summary>
    public class MarketingPack : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public MarketingPackStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // A pack can contain multiple pieces of content.
        public ICollection<Content> Contents { get; set; } = new List<Content>();
    }

    public enum MarketingPackStatus
    {
        Draft,
        Ready,
        Archived
    }
}
