namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para registrar eventos de auditoría.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra un evento de auditoría.
    /// </summary>
    Task LogAsync(
        Guid tenantId,
        string action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string result = "Success",
        string? errorMessage = null,
        string? requestId = null,
        Dictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un evento de auditoría de forma síncrona (para casos críticos).
    /// </summary>
    void LogSync(
        Guid tenantId,
        string action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string result = "Success",
        string? errorMessage = null);
}

