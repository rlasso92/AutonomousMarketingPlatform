using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Servicio para resolver el tenant del request actual.
/// Prioridad: Header > Subdomain > Claim
/// </summary>
public class TenantResolverService : ITenantResolverService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<TenantResolverService> _logger;

    public TenantResolverService(
        IHttpContextAccessor httpContextAccessor,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<TenantResolverService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Guid?> ResolveTenantIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Prioridad 1: Header X-Tenant-Id
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            if (Guid.TryParse(headerValue.ToString(), out var tenantId))
            {
                // Validar que el tenant existe
                if (await ValidateTenantExistsAsync(tenantId))
                {
                    httpContext.Items["ResolvedTenantId"] = tenantId;
                    return tenantId;
                }
            }
        }

        // Prioridad 2: Subdominio
        var host = httpContext.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);
        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenantId = await GetTenantIdBySubdomainAsync(subdomain);
            if (tenantId.HasValue)
            {
                httpContext.Items["ResolvedTenantId"] = tenantId.Value;
                return tenantId;
            }
        }

        // Prioridad 3: Claim del usuario autenticado
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = httpContext.User.FindFirst("TenantId");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
            {
                httpContext.Items["ResolvedTenantId"] = claimTenantId;
                return claimTenantId;
            }
        }

        return null;
    }

    private string? ExtractSubdomain(string host)
    {
        // Ejemplo: tenant1.miapp.com -> tenant1
        var parts = host.Split('.');
        if (parts.Length >= 3)
        {
            return parts[0];
        }
        return null;
    }

    private async Task<Guid?> GetTenantIdBySubdomainAsync(string subdomain)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var tenant = await context.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);

            return tenant?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tenant por subdomain: Subdomain={Subdomain}", subdomain);
            return null;
        }
    }

    private async Task<bool> ValidateTenantExistsAsync(Guid tenantId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var tenant = await context.Tenants.FindAsync(tenantId);
            return tenant != null && tenant.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar tenant: TenantId={TenantId}", tenantId);
            return false;
        }
    }
}

/// <summary>
/// Interfaz para el servicio de resolución de tenant.
/// </summary>
public interface ITenantResolverService
{
    Task<Guid?> ResolveTenantIdAsync();
}


