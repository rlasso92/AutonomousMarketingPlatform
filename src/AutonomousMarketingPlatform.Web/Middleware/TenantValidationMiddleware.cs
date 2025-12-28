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
        // Skip validation para health checks, endpoints públicos y login
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/health") || 
            path.StartsWith("/api/public") || 
            path.StartsWith("/account/login") ||
            path.StartsWith("/account/accessdenied"))
        {
            await _next(context);
            return;
        }

        if (_validationEnabled)
        {
            // Obtener TenantId del request (ya resuelto por el middleware anterior o desde Items)
            var tenantId = context.Items["TenantId"] as Guid?;

            // Si no está en Items, intentar obtenerlo
            if (!tenantId.HasValue)
            {
                tenantId = GetTenantIdFromRequest(context);
            }

            // Si aún no hay tenant y el usuario está autenticado, obtenerlo del claim
            if (!tenantId.HasValue && context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst("TenantId");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
                {
                    tenantId = claimTenantId;
                }
            }

            if (tenantId == null || tenantId == Guid.Empty)
            {
                // Si no está autenticado, redirigir a login
                if (context.User?.Identity?.IsAuthenticated != true)
                {
                    context.Response.Redirect("/Account/Login");
                    return;
                }

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

