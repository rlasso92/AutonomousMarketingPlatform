using System.ComponentModel.DataAnnotations.Schema;
using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class CampaignMetrics : BaseEntity
    {
        // Foreign key to the Campaign
        public Guid CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public Campaign Campaign { get; set; } = null!;

        // Email Metrics
        public int EmailsSent { get; set; }
        public int EmailsOpened { get; set; }
        public int Clicks { get; set; }
        public int Bounces { get; set; }
        public int Unsubscribes { get; set; }

        // Social Media Metrics
        public int Impressions { get; set; }
        public int Likes { get; set; }
        public int Shares { get; set; }
        public int Comments { get; set; }

        // WhatsApp Metrics
        public int MessagesSent { get; set; }
        public int MessagesDelivered { get; set; }
        public int MessagesRead { get; set; }
        public int Replies { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
