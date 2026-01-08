using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities
{
    public class IntegrationSetting : BaseEntity
    {
        public ServiceCategory Category { get; set; }
        public ServiceName Service { get; set; }
        public string DisplayName { get; set; } = null!;
        public string IconClass { get; set; } = null!;
        public bool IsEnabled { get; set; }
        public string EncryptedConfigJson { get; set; } = null!;
    }

    public enum ServiceCategory
    {
        Storage,
        SMS,
        SMTP
        // Add more categories as needed
    }

    public enum ServiceName
    {
        // Storage
        OneDrive,
        GoogleDrive,
        Dropbox,
        AWSS3,
        Cloudflare,
        Box,

        // SMS
        Telegram,
        Slack,
        MSTeams,

        // SMTP
        SendGrid,
        Mailgun,
        SMTPcom,
        GenericSMTP
        // Add more services as needed
    }
}
