using Microsoft.AspNetCore.Mvc;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Infrastructure.Data;
using AutonomousMarketingPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controller para diagnóstico del sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly ITenantResolverService _tenantResolver;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(
        ITenantResolverService tenantResolver,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<DiagnosticController> logger)
    {
        _tenantResolver = tenantResolver;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de diagnóstico para verificar el estado del sistema.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var diagnostics = new
            {
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                host = Request.Host.Host,
                path = Request.Path,
                tenantResolution = new
                {
                    resolvedTenantId = HttpContext.Items["TenantId"]?.ToString(),
                    resolvedFrom = HttpContext.Items["ResolvedTenantId"]?.ToString(),
                    isDefaultTenant = HttpContext.Items["IsDefaultTenant"]?.ToString() == "True",
                    userAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                    userTenantClaim = User?.FindFirst("TenantId")?.Value
                },
                database = await CheckDatabaseAsync(),
                tenants = await GetTenantsInfoAsync()
            };

            return Ok(diagnostics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en diagnóstico");
            return StatusCode(500, new
            {
                error = new
                {
                    message = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                }
            });
        }
    }

    private async Task<object> CheckDatabaseAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var canConnect = await context.Database.CanConnectAsync();
            return new
            {
                connected = canConnect,
                status = canConnect ? "OK" : "ERROR"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                connected = false,
                status = "ERROR",
                error = ex.Message
            };
        }
    }

    private async Task<object> GetTenantsInfoAsync()
    {
        try
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var tenants = await context.Tenants
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    subdomain = t.Subdomain,
                    isDefault = t.Subdomain == "default"
                })
                .ToListAsync();

            return new
            {
                count = tenants.Count,
                tenants = tenants,
                hasDefault = tenants.Any(t => t.isDefault)
            };
        }
        catch (Exception ex)
        {
            return new
            {
                count = 0,
                error = ex.Message
            };
        }
    }
}

