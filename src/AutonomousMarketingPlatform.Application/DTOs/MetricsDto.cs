namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para métricas de campaña.
/// </summary>
public class CampaignMetricsDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public DateTime MetricDate { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Engagement { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public int ActivePosts { get; set; }
    public bool IsManualEntry { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public decimal? ClickThroughRate { get; set; }
    public decimal? EngagementRate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para métricas de publicación individual.
/// </summary>
public class PublishingJobMetricsDto
{
    public Guid Id { get; set; }
    public Guid PublishingJobId { get; set; }
    public string? PublishingJobContent { get; set; }
    public string? Channel { get; set; }
    public DateTime MetricDate { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Engagement { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public decimal? ClickThroughRate { get; set; }
    public decimal? EngagementRate { get; set; }
    public bool IsManualEntry { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para registrar métricas de campaña.
/// </summary>
public class RegisterCampaignMetricsDto
{
    public Guid CampaignId { get; set; }
    public DateTime MetricDate { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para registrar métricas de publicación.
/// </summary>
public class RegisterPublishingJobMetricsDto
{
    public Guid PublishingJobId { get; set; }
    public DateTime MetricDate { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para resumen de métricas de campaña.
/// </summary>
public class CampaignMetricsSummaryDto
{
    public Guid CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long TotalImpressions { get; set; }
    public long TotalClicks { get; set; }
    public long TotalEngagement { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public decimal? AverageClickThroughRate { get; set; }
    public decimal? AverageEngagementRate { get; set; }
    public int TotalPosts { get; set; }
    public int DaysWithMetrics { get; set; }
    public List<CampaignMetricsDto> DailyMetrics { get; set; } = new();
}

/// <summary>
/// DTO para resumen de métricas de publicación.
/// </summary>
public class PublishingJobMetricsSummaryDto
{
    public Guid PublishingJobId { get; set; }
    public string? Content { get; set; }
    public string? Channel { get; set; }
    public DateTime? PublishedDate { get; set; }
    public long TotalImpressions { get; set; }
    public long TotalClicks { get; set; }
    public long TotalEngagement { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public decimal? AverageClickThroughRate { get; set; }
    public decimal? AverageEngagementRate { get; set; }
    public int DaysWithMetrics { get; set; }
    public List<PublishingJobMetricsDto> DailyMetrics { get; set; } = new();
}

