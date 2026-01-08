using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class Campaign : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string HtmlContent { get; set; } = null!;
        public DateTime ScheduledDate { get; set; }
        public CampaignStatus Status { get; set; }
    }

    public enum CampaignStatus
    {
        Draft,
        Scheduled,
        Sent,
        Failed
    }
}
