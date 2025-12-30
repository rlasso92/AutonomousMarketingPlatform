namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO para lista de campañas.
/// </summary>
public class CampaignListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ContentCount { get; set; }
    public int MarketingPackCount { get; set; }
    public int PublishingJobCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? TenantId { get; set; }
}

/// <summary>
/// DTO para crear una campaña.
/// </summary>
public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Dictionary<string, object>? Objectives { get; set; }
    public Dictionary<string, object>? TargetAudience { get; set; }
    public List<string>? TargetChannels { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para actualizar una campaña.
/// </summary>
public class UpdateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Dictionary<string, object>? Objectives { get; set; }
    public Dictionary<string, object>? TargetAudience { get; set; }
    public List<string>? TargetChannels { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para detalle completo de una campaña.
/// </summary>
public class CampaignDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Dictionary<string, object>? Objectives { get; set; }
    public Dictionary<string, object>? TargetAudience { get; set; }
    public List<string>? TargetChannels { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Relaciones
    public List<ContentListItemDto> Contents { get; set; } = new();
    public List<MarketingPackListItemDto> MarketingPacks { get; set; } = new();
    public List<PublishingJobListItemDto> PublishingJobs { get; set; } = new();
}

/// <summary>
/// DTO para item de contenido en lista.
/// </summary>
public class ContentListItemDto
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? Tags { get; set; }
    public Guid? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para item de MarketingPack en lista.
/// </summary>
public class MarketingPackListItemDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public int CopyCount { get; set; }
    public int AssetPromptCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para item de PublishingJob en lista.
/// </summary>
public class PublishingJobListItemDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

