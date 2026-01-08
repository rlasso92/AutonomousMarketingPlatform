using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Content;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NotFoundException = AutonomousMarketingPlatform.Application.UseCases.Campaigns.NotFoundException;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de contenido (imágenes y videos).
/// Requiere rol Marketer, Admin o Owner para cargar contenido.
/// </summary>
[Authorize]
[AuthorizeRole("Marketer", "Admin", "Owner")]
public class ContentController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContentController> _logger;

    public ContentController(IMediator mediator, ILogger<ContentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Vista principal para cargar archivos.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Upload()
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cargar campañas activas del tenant
            var campaignsQuery = new ListCampaignsQuery
            {
                TenantId = tenantId.Value,
                Status = "Active", // Solo campañas activas
                Take = 100 // Límite razonable
            };

            var campaigns = await _mediator.Send(campaignsQuery);
            ViewBag.Campaigns = campaigns;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar vista de upload");
            ViewBag.Campaigns = new List<CampaignListDto>();
            return View();
        }
    }

    /// <summary>
    /// Endpoint para cargar múltiples archivos.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files, [FromForm] Guid? campaignId, [FromForm] string? description, [FromForm] string? tags)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { error = "No se proporcionaron archivos." });
        }

        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return BadRequest(new { error = "Error de autenticación. Por favor, inicie sesión nuevamente." });
            }

            var command = new UploadFilesCommand
            {
                UserId = userId.Value,
                TenantId = tenantId.Value,
                Files = files,
                CampaignId = campaignId,
                Description = description,
                Tags = tags
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar archivos");
            return StatusCode(500, new { error = "Error al procesar los archivos. Por favor, intente nuevamente." });
        }
    }

    /// <summary>
    /// Vista para listar contenido cargado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? campaignId = null, string? contentType = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new ListContentQuery
            {
                TenantId = tenantId.Value,
                CampaignId = campaignId,
                ContentType = contentType
            };

            var content = await _mediator.Send(query);
            
            ViewBag.CampaignId = campaignId;
            ViewBag.ContentType = contentType;
            
            return View(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar contenido");
            return View(new List<ContentListItemDto>());
        }
    }

    /// <summary>
    /// Elimina un contenido (soft delete).
    /// </summary>
    [HttpPost]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
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

            var command = new DeleteContentCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                ContentId = id
            };

            await _mediator.Send(command);

            TempData["SuccessMessage"] = "Contenido eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar contenido {ContentId}", id);
            TempData["ErrorMessage"] = "Error al eliminar el contenido. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

