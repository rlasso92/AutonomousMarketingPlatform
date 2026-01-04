using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.ApplicationLogs;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para visualización de logs de aplicación.
/// Requiere rol Admin, Owner o SuperAdmin.
/// </summary>
[Authorize]
public class ApplicationLogsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationLogsController> _logger;

    public ApplicationLogsController(IMediator mediator, ILogger<ApplicationLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista de logs de aplicación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? tenantId = null,
        Guid? userId = null,
        string? level = null,
        string? source = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50)
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var currentTenantId = UserHelper.GetTenantId(User);
            
            // Si no es SuperAdmin, usar el tenant del usuario actual
            if (!isSuperAdmin)
            {
                tenantId = currentTenantId;
            }

            var skip = (page - 1) * pageSize;

            var query = new ListApplicationLogsQuery
            {
                TenantId = tenantId,
                UserId = userId,
                Level = level,
                Source = source,
                FromDate = fromDate,
                ToDate = toDate ?? DateTime.UtcNow,
                Skip = skip,
                Take = pageSize,
                IsSuperAdmin = isSuperAdmin
            };

            var logs = await _mediator.Send(query);

            // Cargar lista de tenants si es SuperAdmin
            if (isSuperAdmin)
            {
                var tenantsQuery = new ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
            }

            ViewBag.TenantId = tenantId;
            ViewBag.UserId = userId;
            ViewBag.Level = level;
            ViewBag.Source = source;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.IsSuperAdmin = isSuperAdmin;

            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar logs de aplicación");
            return View(new List<ApplicationLogListDto>());
        }
    }

    /// <summary>
    /// Detalles de un log de aplicación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var tenantId = UserHelper.GetTenantId(User);

            var query = new GetApplicationLogQuery
            {
                Id = id,
                TenantId = tenantId,
                IsSuperAdmin = isSuperAdmin
            };

            var log = await _mediator.Send(query);

            if (log == null)
            {
                TempData["ErrorMessage"] = "Log no encontrado o no tiene permisos para verlo.";
                return RedirectToAction(nameof(Index));
            }

            return View(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles del log {LogId}", id);
            TempData["ErrorMessage"] = "Error al cargar los detalles del log.";
            return RedirectToAction(nameof(Index));
        }
    }
}

