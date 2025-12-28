using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de auditoría.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IRepository<AuditLog> auditRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Obtener información del request si está disponible
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                ipAddress ??= httpContext.Connection.RemoteIpAddress?.ToString();
                userAgent ??= httpContext.Request.Headers["User-Agent"].ToString();
                requestId ??= httpContext.TraceIdentifier;
            }

            // Sanitizar datos sensibles
            oldValues = SanitizeSensitiveData(oldValues);
            newValues = SanitizeSensitiveData(newValues);

            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Result = result,
                ErrorMessage = errorMessage,
                RequestId = requestId,
                AdditionalData = additionalData != null && additionalData.Any() ? JsonSerializer.Serialize(additionalData) : null
            };

            await _auditRepository.AddAsync(auditLog, cancellationToken);

            // También loguear en sistema de logs
            _logger.LogInformation(
                "Audit: Action={Action}, EntityType={EntityType}, EntityId={EntityId}, TenantId={TenantId}, UserId={UserId}, Result={Result}",
                action, entityType, entityId, tenantId, userId, result);
        }
        catch (Exception ex)
        {
            // No fallar la operación principal si la auditoría falla
            _logger.LogError(ex, "Error al registrar auditoría: Action={Action}, EntityType={EntityType}", action, entityType);
        }
    }

    public void LogSync(
        Guid tenantId,
        string action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string result = "Success",
        string? errorMessage = null)
    {
        // Para casos críticos, loguear síncronamente
        _logger.LogWarning(
            "Audit (Sync): Action={Action}, EntityType={EntityType}, EntityId={EntityId}, TenantId={TenantId}, UserId={UserId}, Result={Result}, Error={Error}",
            action, entityType, entityId, tenantId, userId, result, errorMessage);
    }

    private string? SanitizeSensitiveData(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // Eliminar datos sensibles antes de guardar
        var sensitivePatterns = new[]
        {
            (@"password["":\s]*[:=]\s*[""']([^""']+)[""']", "[REDACTED]"),
            (@"api[_-]?key["":\s]*[:=]\s*[""']([^""']+)[""']", "[REDACTED]"),
            (@"token["":\s]*[:=]\s*[""']([^""']+)[""']", "[REDACTED]"),
            (@"secret["":\s]*[:=]\s*[""']([^""']+)[""']", "[REDACTED]")
        };

        var sanitized = data;
        foreach (var (pattern, replacement) in sensitivePatterns)
        {
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return sanitized;
    }
}

