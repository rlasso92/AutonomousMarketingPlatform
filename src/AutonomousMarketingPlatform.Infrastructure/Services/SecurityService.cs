using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de seguridad.
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly IRepository<User> _userRepository;
    private readonly IDbContextFactory<Data.ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<SecurityService> _logger;

    public SecurityService(
        IRepository<User> userRepository,
        IDbContextFactory<Data.ApplicationDbContext> dbContextFactory,
        ILogger<SecurityService> logger)
    {
        _userRepository = userRepository;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<bool> ValidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var tenant = await context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
            return tenant != null && tenant.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al validar tenant: TenantId={TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> ValidateUserBelongsToTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, tenantId, cancellationToken);
            return user != null && user.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al validar usuario-tenant: UserId={UserId}, TenantId={TenantId}", userId, tenantId);
            return false;
        }
    }

    public async Task<bool> ValidateEntityAccessAsync<T>(Guid entityId, Guid tenantId, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            var entity = await context.Set<T>()
                .FindAsync(new object[] { entityId }, cancellationToken);

            if (entity == null)
                return false;

            // Verificar si la entidad implementa ITenantEntity
            if (entity is Domain.Common.ITenantEntity tenantEntity)
            {
                return tenantEntity.TenantId == tenantId;
            }

            return true; // Si no es multi-tenant, permitir acceso
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al validar acceso a entidad: EntityType={EntityType}, EntityId={EntityId}, TenantId={TenantId}",
                typeof(T).Name, entityId, tenantId);
            return false;
        }
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Eliminar scripts y tags peligrosos
        var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=", "", RegexOptions.IgnoreCase);

        return sanitized;
    }

    public bool IsValidUrl(string url, out string? sanitizedUrl)
    {
        sanitizedUrl = null;

        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Validar formato de URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Solo permitir HTTP y HTTPS
        if (uri.Scheme != "http" && uri.Scheme != "https")
            return false;

        // Sanitizar
        sanitizedUrl = uri.ToString();
        return true;
    }
}

