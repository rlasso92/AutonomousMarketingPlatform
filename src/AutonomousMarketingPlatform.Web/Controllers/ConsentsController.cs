using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Consents;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de consentimientos.
/// </summary>
[Authorize]
public class ConsentsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsentsController> _logger;

    public ConsentsController(IMediator mediator, ILogger<ConsentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los consentimientos del usuario actual.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);

        if (!userId.HasValue || !tenantId.HasValue)
        {
            _logger.LogWarning("Usuario autenticado sin UserId o TenantId");
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var query = new GetUserConsentsQuery
            {
                UserId = userId.Value,
                TenantId = tenantId.Value
            };

            var result = await _mediator.Send(query);
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consentimientos del usuario {UserId}", userId.Value);
            TempData["ErrorMessage"] = "Error al cargar los consentimientos. Por favor, intente nuevamente.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Otorga un consentimiento.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grant([FromForm] CreateConsentDto dto)
    {
        var userId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);

        if (!userId.HasValue || !tenantId.HasValue)
        {
            TempData["ErrorMessage"] = "Error de autenticación. Por favor, inicie sesión nuevamente.";
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var command = new GrantConsentCommand
            {
                UserId = userId.Value,
                TenantId = tenantId.Value,
                ConsentType = dto.ConsentType,
                ConsentVersion = dto.ConsentVersion ?? "1.0",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var result = await _mediator.Send(command);
            TempData["SuccessMessage"] = $"Consentimiento '{dto.ConsentType}' otorgado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al otorgar consentimiento {ConsentType} para usuario {UserId}", dto.ConsentType, userId.Value);
            TempData["ErrorMessage"] = "Error al otorgar el consentimiento. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Revoca un consentimiento.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke([FromForm] string consentType)
    {
        var userId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);

        if (!userId.HasValue || !tenantId.HasValue)
        {
            TempData["ErrorMessage"] = "Error de autenticación. Por favor, inicie sesión nuevamente.";
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var command = new RevokeConsentCommand
            {
                UserId = userId.Value,
                TenantId = tenantId.Value,
                ConsentType = consentType
            };

            var result = await _mediator.Send(command);
            TempData["SuccessMessage"] = $"Consentimiento '{consentType}' revocado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Intento de revocar consentimiento requerido {ConsentType}", consentType);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar consentimiento {ConsentType} para usuario {UserId}", consentType, userId.Value);
            TempData["ErrorMessage"] = "Error al revocar el consentimiento. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

