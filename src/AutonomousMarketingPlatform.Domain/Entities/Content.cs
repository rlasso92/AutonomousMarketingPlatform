using System.ComponentModel.DataAnnotations.Schema;
using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class Content : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public ContentType ContentType { get; set; }
        public ContentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key for Campaign
        public Guid? CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public Campaign? Campaign { get; set; }
    }

    public enum ContentType
    {
        BlogPost,
        SocialMediaUpdate,
        Advertisement,
        EmailBody
    }

    public enum ContentStatus
    {
        Draft,
        InReview,
        Published,
        Archived
    }
}
