namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para integrar con automatizaciones externas (n8n).
/// </summary>
public interface IExternalAutomationService
{
    /// <summary>
    /// Dispara una automatización externa cuando ocurre un evento.
    /// </summary>
    Task<string> TriggerAutomationAsync(
        Guid tenantId,
        string eventType,
        object eventData,
        Guid? userId = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? additionalContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el estado de una automatización en ejecución.
    /// </summary>
    Task<AutomationExecutionStatus> GetExecutionStatusAsync(
        Guid tenantId,
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela una automatización en ejecución.
    /// </summary>
    Task<bool> CancelExecutionAsync(
        Guid tenantId,
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa una respuesta recibida de n8n (webhook).
    /// </summary>
    Task ProcessWebhookResponseAsync(
        Guid tenantId,
        string requestId,
        WebhookResponseData responseData,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Estado de una ejecución de automatización externa.
/// </summary>
public class AutomationExecutionStatus
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Queued, InProgress, Completed, Failed, Timeout, Cancelled, Retrying
    public int? Progress { get; set; } // 0-100
    public string? CurrentStep { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public object? ResultData { get; set; }
}

/// <summary>
/// Datos de respuesta recibidos de n8n vía webhook.
/// </summary>
public class WebhookResponseData
{
    public string RequestId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? ResultData { get; set; }
    public object? GeneratedContent { get; set; }
    public object? Insights { get; set; }
    public object? Recommendations { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public bool? Retryable { get; set; }
    public int? Progress { get; set; }
    public string? CurrentStep { get; set; }
    public string? Signature { get; set; } // Para validar autenticidad
}

