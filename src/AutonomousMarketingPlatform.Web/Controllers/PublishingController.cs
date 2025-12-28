using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Publishing;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de publicaciones.
/// </summary>
[Authorize]
public class PublishingController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<PublishingController> _logger;

    public PublishingController(IMediator mediator, ILogger<PublishingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas las publicaciones del tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? campaignId = null, string? status = null, string? channel = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new ListPublishingJobsQuery
            {
                TenantId = tenantId.Value,
                CampaignId = campaignId,
                Status = status,
                Channel = channel
            };

            var jobs = await _mediator.Send(query);
            ViewBag.CampaignId = campaignId;
            ViewBag.StatusFilter = status;
            ViewBag.ChannelFilter = channel;
            return View(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar publicaciones");
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra el formulario para generar una nueva publicación.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public IActionResult Generate(Guid? campaignId = null, Guid? marketingPackId = null)
    {
        ViewBag.CampaignId = campaignId;
        ViewBag.MarketingPackId = marketingPackId;
        return View(new GeneratePublishingJobDto
        {
            CampaignId = campaignId ?? Guid.Empty,
            MarketingPackId = marketingPackId ?? Guid.Empty,
            RequiresApproval = true
        });
    }

    /// <summary>
    /// Genera un nuevo trabajo de publicación.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Generate(GeneratePublishingJobDto model)
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

            var command = new GeneratePublishingJobCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                CampaignId = model.CampaignId,
                MarketingPackId = model.MarketingPackId,
                GeneratedCopyId = model.GeneratedCopyId,
                Channel = model.Channel,
                ScheduledDate = model.ScheduledDate,
                RequiresApproval = model.RequiresApproval
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Publicación generada exitosamente. Se procesará en breve.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar publicación");
            ModelState.AddModelError(string.Empty, $"Error al generar la publicación: {ex.Message}");
            return View(model);
        }
    }

    /// <summary>
    /// Muestra el detalle de una publicación.
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

            var query = new GetPublishingJobQuery
            {
                TenantId = tenantId.Value,
                JobId = id
            };

            var job = await _mediator.Send(query);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener publicación {JobId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Descarga el paquete de publicación (Fase A).
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> DownloadPackage(Guid id)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetPublishingJobQuery
            {
                TenantId = tenantId.Value,
                JobId = id
            };

            var job = await _mediator.Send(query);

            if (job == null || string.IsNullOrEmpty(job.DownloadUrl))
            {
                return NotFound();
            }

            // Parsear data URI
            if (job.DownloadUrl.StartsWith("data:application/json;base64,"))
            {
                var base64 = job.DownloadUrl.Substring("data:application/json;base64,".Length);
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                
                return File(
                    System.Text.Encoding.UTF8.GetBytes(json),
                    "application/json",
                    $"publicacion-{id}.json");
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar paquete {JobId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Aprueba una publicación (marca como publicada manualmente).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Approve(Guid id, string? publishedUrl, string? externalPostId)
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new ApprovePublishingJobCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                JobId = id,
                PublishedUrl = publishedUrl,
                ExternalPostId = externalPostId
            };

            await _mediator.Send(command);

            TempData["SuccessMessage"] = "Publicación aprobada y marcada como publicada.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar publicación {JobId}", id);
            TempData["ErrorMessage"] = $"Error al aprobar la publicación: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

