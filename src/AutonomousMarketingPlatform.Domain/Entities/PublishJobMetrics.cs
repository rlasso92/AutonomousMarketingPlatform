using System.ComponentModel.DataAnnotations.Schema;
using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class PublishJobMetrics : BaseEntity
    {
        // Foreign key to the PublishJob
        public Guid PublishJobId { get; set; }
        [ForeignKey("PublishJobId")]
        public PublishJob PublishJob { get; set; } = null!;

        // Metrics for this specific job
        public int Interactions { get; set; } // e.g., Clicks, Likes, Replies
        public int Errors { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
