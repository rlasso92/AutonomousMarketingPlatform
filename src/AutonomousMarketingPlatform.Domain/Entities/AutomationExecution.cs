using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Entidad para registrar ejecuciones de automatizaciones externas (n8n).
/// </summary>
public class AutomationExecution : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string RequestId { get; set; } = string.Empty; // ID único de la solicitud
    public string WorkflowId { get; set; } = string.Empty; // ID del workflow en n8n
    public string EventType { get; set; } = string.Empty; // Tipo de evento que disparó la automatización
    public string Status { get; set; } = "Pending"; // Pending, Queued, InProgress, Completed, Failed, Timeout, Cancelled, Retrying
    public string? DataSent { get; set; } // JSON con datos enviados
    public string? DataReceived { get; set; } // JSON con datos recibidos
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int RetryCount { get; set; }
    public int? Progress { get; set; } // 0-100
    public string? CurrentStep { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public Guid? UserId { get; set; } // Usuario que disparó el evento
    public Guid? RelatedEntityId { get; set; } // ID de la entidad relacionada (Content, Campaign, etc.)
    public string? RelatedEntityType { get; set; } // Tipo de entidad relacionada
}

