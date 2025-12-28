using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de campañas.
/// </summary>
[Authorize]
public class CampaignsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(IMediator mediator, ILogger<CampaignsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas las campañas del tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new ListCampaignsQuery
            {
                TenantId = tenantId.Value,
                Status = status
            };

            var campaigns = await _mediator.Send(query);
            ViewBag.StatusFilter = status;
            return View(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar campañas");
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra el formulario para crear una nueva campaña.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public IActionResult Create()
    {
        return View(new CreateCampaignDto { Status = "Draft" });
    }

    /// <summary>
    /// Crea una nueva campaña.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Create(CreateCampaignDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new CreateCampaignCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                Campaign = model
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Campaña creada exitosamente.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear campaña");
            ModelState.AddModelError(string.Empty, "Error al crear la campaña. Por favor, intente nuevamente.");
            return View(model);
        }
    }

    /// <summary>
    /// Muestra el detalle de una campaña.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetCampaignQuery
            {
                TenantId = tenantId.Value,
                CampaignId = id
            };

            var campaign = await _mediator.Send(query);

            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campaña {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra el formulario para editar una campaña.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetCampaignQuery
            {
                TenantId = tenantId.Value,
                CampaignId = id
            };

            var campaign = await _mediator.Send(query);

            if (campaign == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateCampaignDto
            {
                Name = campaign.Name,
                Description = campaign.Description,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Budget = campaign.Budget,
                Objectives = campaign.Objectives,
                TargetAudience = campaign.TargetAudience,
                TargetChannels = campaign.TargetChannels ?? new List<string>(),
                Notes = campaign.Notes
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campaña para editar {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Actualiza una campaña existente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Edit(Guid id, UpdateCampaignDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new UpdateCampaignCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                CampaignId = id,
                Campaign = model
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Campaña actualizada exitosamente.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar campaña {CampaignId}", id);
            ModelState.AddModelError(string.Empty, "Error al actualizar la campaña. Por favor, intente nuevamente.");
            return View(model);
        }
    }

    /// <summary>
    /// Elimina una campaña (soft delete).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new DeleteCampaignCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                CampaignId = id
            };

            await _mediator.Send(command);

            TempData["SuccessMessage"] = "Campaña eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar campaña {CampaignId}", id);
            TempData["ErrorMessage"] = "Error al eliminar la campaña. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

