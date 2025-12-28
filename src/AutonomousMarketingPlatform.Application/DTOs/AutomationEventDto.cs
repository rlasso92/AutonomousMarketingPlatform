namespace AutonomousMarketingPlatform.Application.DTOs;

/// <summary>
/// DTO base para eventos que disparan automatizaciones externas.
/// </summary>
public class AutomationEventDto
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string WorkflowId { get; set; } = string.Empty;
    public string? WorkflowVersion { get; set; }
}

/// <summary>
/// DTO para evento de contenido cargado.
/// </summary>
public class ContentUploadedEventDto : AutomationEventDto
{
    public Guid ContentId { get; set; }
    public string ContentType { get; set; } = string.Empty; // Image, Video
    public string FileUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public bool IsAiGenerated { get; set; }
    public Guid? CampaignId { get; set; }
    public List<string>? Tags { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO para evento de campaña creada/activada.
/// </summary>
public class CampaignEventDto : AutomationEventDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignStatus { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public decimal? SpentAmount { get; set; }
    public List<Guid>? ContentIds { get; set; }
    public string? TargetAudience { get; set; }
    public string? MarketingStrategy { get; set; }
}

/// <summary>
/// DTO para evento de automatización interna.
/// </summary>
public class InternalAutomationEventDto : AutomationEventDto
{
    public Guid AutomationId { get; set; }
    public string AutomationType { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public DateTime? LastExecutionAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO para evento programado (scheduled).
/// </summary>
public class ScheduledEventDto : AutomationEventDto
{
    public string ScheduleType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Custom
    public DateTime ScheduledTime { get; set; }
    public Dictionary<string, object>? ScheduleParameters { get; set; }
}

/// <summary>
/// DTO para evento de feedback del usuario.
/// </summary>
public class UserFeedbackEventDto : AutomationEventDto
{
    public Guid? ContentId { get; set; }
    public Guid? CampaignId { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public bool IsPositive { get; set; }
    public Dictionary<string, object>? FeedbackContext { get; set; }
}

/// <summary>
/// DTO completo para enviar a n8n.
/// </summary>
public class AutomationRequestDto
{
    public AutomationEventDto Event { get; set; } = null!;
    public Dictionary<string, object>? EventData { get; set; }
    public Dictionary<string, object>? UserPreferences { get; set; }
    public Dictionary<string, object>? MemoryContext { get; set; }
    public Dictionary<string, object>? SystemMetrics { get; set; }
    public AutomationConfigDto AutomationConfig { get; set; } = new();
}

/// <summary>
/// Configuración de la automatización.
/// </summary>
public class AutomationConfigDto
{
    public string WorkflowId { get; set; } = string.Empty;
    public string? WorkflowVersion { get; set; }
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 300;
    public string? ExpectedResponse { get; set; }
}

