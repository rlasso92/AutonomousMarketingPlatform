using AutonomousMarketingPlatform.Domain.Common;

namespace AutonomousMarketingPlatform.Domain.Entities;

/// <summary>
/// Entidad para persistir logs de aplicación en la base de datos.
/// Permite rastrear errores, warnings e información crítica del sistema.
/// </summary>
public class ApplicationLog : BaseEntity
{
    /// <summary>
    /// Nivel del log (Error, Warning, Information, Debug, Critical).
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje del log.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del tenant (opcional, puede ser null para logs del sistema).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Identificador del usuario (opcional, puede ser null para logs del sistema).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Origen del log (ej: "AccountController", "TenantResolver", "CampaignService").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace completo (solo para errores).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Tipo de excepción (si aplica).
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Mensaje de excepción interna (si aplica).
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// Request ID para correlación con otros logs del mismo request.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Path del request HTTP (si aplica).
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Método HTTP (GET, POST, etc.) si aplica.
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Datos adicionales en formato JSON (para contexto adicional).
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// IP address del cliente (si aplica).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent del cliente (si aplica).
    /// </summary>
    public string? UserAgent { get; set; }
}


