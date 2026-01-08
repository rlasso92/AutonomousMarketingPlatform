using System.ComponentModel.DataAnnotations.Schema;
using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    /// <summary>
    /// Represents a job to publish a specific piece of content to a channel at a scheduled time.
    /// </summary>
    public class PublishJob : BaseEntity
    {
        public string Channel { get; set; } = null!; // e.g., "Email", "Facebook", "Twitter"
        public DateTime ScheduledTime { get; set; }
        public PublishJobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key for the content to be published
        public Guid ContentId { get; set; }
        [ForeignKey("ContentId")]
        public Content Content { get; set; } = null!;

        // Optional: Foreign key for a campaign this job belongs to
        public Guid? CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }
    }

    public enum PublishJobStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
}
