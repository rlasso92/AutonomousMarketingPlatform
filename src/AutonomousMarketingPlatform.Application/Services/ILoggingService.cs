namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para persistir logs de aplicación en la base de datos.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Persiste un log en la base de datos de forma asíncrona.
    /// </summary>
    /// <param name="level">Nivel del log (Error, Warning, Information, Debug, Critical).</param>
    /// <param name="message">Mensaje del log.</param>
    /// <param name="source">Origen del log (ej: "AccountController", "TenantResolver").</param>
    /// <param name="tenantId">Identificador del tenant (opcional).</param>
    /// <param name="userId">Identificador del usuario (opcional).</param>
    /// <param name="exception">Excepción asociada (opcional).</param>
    /// <param name="requestId">Request ID para correlación (opcional).</param>
    /// <param name="path">Path del request HTTP (opcional).</param>
    /// <param name="httpMethod">Método HTTP (opcional).</param>
    /// <param name="additionalData">Datos adicionales en formato JSON (opcional).</param>
    /// <param name="ipAddress">IP address del cliente (opcional).</param>
    /// <param name="userAgent">User Agent del cliente (opcional).</param>
    Task LogAsync(
        string level,
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        Exception? exception = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Persiste un log de error.
    /// </summary>
    Task LogErrorAsync(
        string message,
        string source,
        Exception? exception = null,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Persiste un log de warning.
    /// </summary>
    Task LogWarningAsync(
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Persiste un log de información.
    /// </summary>
    Task LogInformationAsync(
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Persiste un log crítico.
    /// </summary>
    Task LogCriticalAsync(
        string message,
        string source,
        Exception? exception = null,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null);
}


