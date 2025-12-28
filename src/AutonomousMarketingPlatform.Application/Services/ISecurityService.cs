namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para validaciones de seguridad y multi-tenant.
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Valida que el tenant existe y está activo.
    /// </summary>
    Task<bool> ValidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida que el usuario pertenece al tenant.
    /// </summary>
    Task<bool> ValidateUserBelongsToTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida acceso a una entidad del tenant.
    /// </summary>
    Task<bool> ValidateEntityAccessAsync<T>(Guid entityId, Guid tenantId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sanitiza datos de entrada para prevenir XSS.
    /// </summary>
    string SanitizeInput(string input);

    /// <summary>
    /// Valida y sanitiza una URL.
    /// </summary>
    bool IsValidUrl(string url, out string? sanitizedUrl);
}

