namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para trabajo de publicación.
/// </summary>
public class PublishingJobDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid? MarketingPackId { get; set; }
    public Guid? GeneratedCopyId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? PublishedUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public string? MediaUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public bool RequiresApproval { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para lista de trabajos de publicación.
/// </summary>
public class PublishingJobListDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresApproval { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para generar publicación.
/// </summary>
public class GeneratePublishingJobDto
{
    public Guid CampaignId { get; set; }
    public Guid MarketingPackId { get; set; }
    public Guid? GeneratedCopyId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool RequiresApproval { get; set; } = true;
}

