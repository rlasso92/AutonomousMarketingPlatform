using System.ComponentModel.DataAnnotations.Schema;
using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class Consent : BaseEntity
    {
        public ConsentType ConsentType { get; set; }
        public ConsentStatus Status { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Foreign key for Contact
        public Guid ContactId { get; set; }
        [ForeignKey("ContactId")]
        public Contact Contact { get; set; } = null!;
    }

    public enum ConsentType
    {
        EmailNewsletter,
        SmsAlerts,
        ThirdPartyMarketing
    }

    public enum ConsentStatus
    {
        Granted,
        Revoked
    }
}
