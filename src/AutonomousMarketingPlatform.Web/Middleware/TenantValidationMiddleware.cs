using AutonomousMarketingPlatform.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Web.Middleware;

/// <summary>
/// Middleware para validar tenant en cada request.
/// </summary>
public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecurityService _securityService;
    private readonly ILogger<TenantValidationMiddleware> _logger;
    private readonly bool _validationEnabled;

    public TenantValidationMiddleware(
        RequestDelegate next,
        ISecurityService securityService,
        ILogger<TenantValidationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _securityService = securityService;
        _logger = logger;
        _validationEnabled = configuration.GetValue<bool>("MultiTenant:ValidationEnabled", true);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation para health checks y endpoints públicos
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/health") || path.StartsWith("/api/public"))
        {
            await _next(context);
            return;
        }

        if (_validationEnabled)
        {
            // Obtener TenantId del request (header, claim, subdomain, etc.)
            var tenantId = GetTenantIdFromRequest(context);

            if (tenantId == null || tenantId == Guid.Empty)
            {
                _logger.LogWarning("Request sin TenantId: Path={Path}, IP={IP}",
                    context.Request.Path, context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "MISSING_TENANT",
                        message = "TenantId es requerido"
                    }
                });
                return;
            }

            // Validar que el tenant existe y está activo
            var isValid = await _securityService.ValidateTenantAsync(tenantId.Value);
            if (!isValid)
            {
                _logger.LogWarning("Intento de acceso con tenant inválido: TenantId={TenantId}, Path={Path}, IP={IP}",
                    tenantId, context.Request.Path, context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INVALID_TENANT",
                        message = "Tenant no válido o inactivo"
                    }
                });
                return;
            }

            // Agregar TenantId al contexto para uso posterior
            context.Items["TenantId"] = tenantId.Value;
        }

        await _next(context);
    }

    private Guid? GetTenantIdFromRequest(HttpContext context)
    {
        // 1. Intentar desde header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            if (Guid.TryParse(headerValue.ToString(), out var tenantId))
                return tenantId;
        }

        // 2. Intentar desde claims (si está autenticado)
        var tenantClaim = context.User?.FindFirst("TenantId");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
            return claimTenantId;

        // 3. Intentar desde subdomain
        var host = context.Request.Host.Host;
        // TODO: Implementar resolución por subdomain si es necesario

        return null;
    }
}

