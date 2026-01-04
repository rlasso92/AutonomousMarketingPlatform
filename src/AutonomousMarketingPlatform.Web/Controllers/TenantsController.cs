using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de tenants.
/// Requiere rol Owner o SuperAdmin.
/// </summary>
[Authorize]
[AuthorizeRole("Owner", "SuperAdmin")]
public class TenantsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(IMediator mediator, ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista de tenants.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var query = new ListTenantsQuery();
            var tenants = await _mediator.Send(query);
            return View(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar tenants");
            return View(new List<TenantDto>());
        }
    }

    /// <summary>
    /// Formulario para crear tenant.
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTenantDto());
    }

    /// <summary>
    /// Crear tenant.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTenantDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new CreateTenantCommand
            {
                Name = model.Name,
                Subdomain = model.Subdomain,
                ContactEmail = model.ContactEmail
            };

            await _mediator.Send(command);
            TempData["SuccessMessage"] = "Tenant creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tenant");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Muestra el formulario para editar un tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var query = new GetTenantQuery
            {
                TenantId = id
            };

            var tenant = await _mediator.Send(query);

            if (tenant == null)
            {
                TempData["ErrorMessage"] = "Tenant no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateTenantDto
            {
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                ContactEmail = tenant.ContactEmail,
                IsActive = tenant.IsActive
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tenant para editar {TenantId}", id);
            TempData["ErrorMessage"] = "Error al cargar el tenant para editar.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Actualiza un tenant existente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateTenantDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new UpdateTenantCommand
            {
                TenantId = id,
                Tenant = model
            };

            await _mediator.Send(command);
            TempData["SuccessMessage"] = "Tenant actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tenant {TenantId}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}

