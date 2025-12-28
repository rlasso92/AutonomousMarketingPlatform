using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Metrics;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de métricas de campañas y publicaciones.
/// </summary>
[Authorize]
public class MetricsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMediator mediator, ILogger<MetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista métricas de todas las campañas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new ListCampaignsMetricsQuery
            {
                TenantId = tenantId.Value,
                FromDate = fromDate,
                ToDate = toDate ?? DateTime.UtcNow
            };

            var metrics = await _mediator.Send(query);
            
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            
            return View(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar métricas de campañas");
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra métricas detalladas de una campaña.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Campaign(Guid campaignId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetCampaignMetricsQuery
            {
                TenantId = tenantId.Value,
                CampaignId = campaignId,
                FromDate = fromDate,
                ToDate = toDate ?? DateTime.UtcNow
            };

            var summary = await _mediator.Send(query);
            
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            
            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de campaña {CampaignId}", campaignId);
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra métricas detalladas de una publicación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PublishingJob(Guid publishingJobId)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetPublishingJobMetricsQuery
            {
                TenantId = tenantId.Value,
                PublishingJobId = publishingJobId
            };

            var summary = await _mediator.Send(query);
            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de publicación {PublishingJobId}", publishingJobId);
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra formulario para registrar métricas de campaña manualmente.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public IActionResult RegisterCampaign(Guid campaignId)
    {
        ViewBag.CampaignId = campaignId;
        return View(new RegisterCampaignMetricsDto
        {
            CampaignId = campaignId,
            MetricDate = DateTime.UtcNow.Date
        });
    }

    /// <summary>
    /// Registra métricas de campaña.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> RegisterCampaign(RegisterCampaignMetricsDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CampaignId = model.CampaignId;
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

            var command = new RegisterCampaignMetricsCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                Metrics = model
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Métricas registradas correctamente";
            return RedirectToAction("Campaign", new { campaignId = model.CampaignId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar métricas de campaña");
            ModelState.AddModelError(string.Empty, "Error al registrar métricas. Por favor, intente nuevamente.");
            ViewBag.CampaignId = model.CampaignId;
            return View(model);
        }
    }

    /// <summary>
    /// Muestra formulario para registrar métricas de publicación manualmente.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public IActionResult RegisterPublishingJob(Guid publishingJobId)
    {
        ViewBag.PublishingJobId = publishingJobId;
        return View(new RegisterPublishingJobMetricsDto
        {
            PublishingJobId = publishingJobId,
            MetricDate = DateTime.UtcNow.Date
        });
    }

    /// <summary>
    /// Registra métricas de publicación.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> RegisterPublishingJob(RegisterPublishingJobMetricsDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.PublishingJobId = model.PublishingJobId;
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

            var command = new RegisterPublishingJobMetricsCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                Metrics = model
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Métricas registradas correctamente";
            return RedirectToAction("PublishingJob", new { publishingJobId = model.PublishingJobId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar métricas de publicación");
            ModelState.AddModelError(string.Empty, "Error al registrar métricas. Por favor, intente nuevamente.");
            ViewBag.PublishingJobId = model.PublishingJobId;
            return View(model);
        }
    }
}

