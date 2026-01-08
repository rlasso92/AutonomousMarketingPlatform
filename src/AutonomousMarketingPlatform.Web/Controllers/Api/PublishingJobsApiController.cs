using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

/// <summary>
/// Controlador API para gestión de PublishingJobs.
/// Usado por workflows n8n para guardar resultados de publicaciones.
/// </summary>
/// <remarks>
/// NOTA DE SEGURIDAD: En producción, este endpoint debería tener autenticación por API key
/// o estar protegido por una red privada. Por ahora se permite acceso sin autenticación
/// para facilitar la integración con n8n en desarrollo.
/// </remarks>
[ApiController]
[Route("api/publishing-jobs")]
[AllowAnonymous]
public class PublishingJobsApiController : ControllerBase
{
    private readonly IRepository<PublishingJob> _publishingJobRepository;
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishingJobsApiController> _logger;

    public PublishingJobsApiController(
        IRepository<PublishingJob> publishingJobRepository,
        IRepository<Campaign> campaignRepository,
        IUnitOfWork unitOfWork,
        ILogger<PublishingJobsApiController> logger)
    {
        _publishingJobRepository = publishingJobRepository;
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene PublishingJobs con filtros.
    /// Endpoint usado por workflows n8n para obtener jobs publicados.
    /// </summary>
    /// <param name="tenantId">ID del tenant (requerido)</param>
    /// <param name="publishedAfter">Fecha mínima de publicación (opcional)</param>
    /// <param name="status">Estado del job (opcional, ej: "Success")</param>
    /// <param name="campaignId">ID de la campaña (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de PublishingJobs</returns>
    /// <response code="200">Lista de PublishingJobs</response>
    /// <response code="400">Si los parámetros son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PublishingJobResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublishingJobs(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime? publishedAfter = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest(new { error = "tenantId is required and must be a valid GUID" });
            }

            var allJobs = await _publishingJobRepository.GetAllAsync(tenantId, cancellationToken);

            // Aplicar filtros
            var filteredJobs = allJobs.AsEnumerable();

            if (publishedAfter.HasValue)
            {
                filteredJobs = filteredJobs.Where(j => j.PublishedDate.HasValue && 
                    j.PublishedDate.Value >= publishedAfter.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredJobs = filteredJobs.Where(j => j.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (campaignId.HasValue && campaignId.Value != Guid.Empty)
            {
                filteredJobs = filteredJobs.Where(j => j.CampaignId == campaignId.Value);
            }

            var results = filteredJobs.OrderByDescending(j => j.PublishedDate ?? j.CreatedAt).ToList();

            var responses = results.Select(job => new PublishingJobResponse
            {
                Id = job.Id,
                TenantId = job.TenantId,
                CampaignId = job.CampaignId,
                MarketingPackId = job.MarketingPackId,
                GeneratedCopyId = job.GeneratedCopyId,
                Channel = job.Channel,
                Status = job.Status,
                PublishedDate = job.PublishedDate,
                PublishedUrl = job.PublishedUrl,
                ExternalPostId = job.ExternalPostId,
                CreatedAt = job.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener PublishingJobs");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Internal server error",
                    message = "Failed to get publishing jobs"
                });
        }
    }

    /// <summary>
    /// Crea un PublishingJob desde n8n después de publicar contenido.
    /// Endpoint usado por workflows n8n para guardar resultados de publicaciones.
    /// </summary>
    /// <param name="request">Datos del PublishingJob a guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>PublishingJob guardado</returns>
    /// <response code="200">PublishingJob guardado exitosamente</response>
    /// <response code="400">Si los datos son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(PublishingJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePublishingJob(
        [FromBody] CreatePublishingJobRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar datos requeridos
            if (request.TenantId == Guid.Empty)
            {
                _logger.LogWarning("CreatePublishingJob llamado con tenantId vacío");
                return BadRequest(new { error = "tenantId is required and must be a valid GUID" });
            }

            if (request.CampaignId == Guid.Empty)
            {
                _logger.LogWarning("CreatePublishingJob llamado con campaignId vacío");
                return BadRequest(new { error = "campaignId is required and must be a valid GUID" });
            }

            if (string.IsNullOrWhiteSpace(request.Channel))
            {
                return BadRequest(new { error = "channel is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { error = "content is required" });
            }

            // Validar que la campaña existe y pertenece al tenant correcto
            var campaign = await _campaignRepository.GetByIdAsync(
                request.CampaignId, request.TenantId, cancellationToken);
            
            if (campaign == null)
            {
                _logger.LogWarning(
                    "CreatePublishingJob: Campaña {CampaignId} no encontrada para tenant {TenantId}",
                    request.CampaignId, request.TenantId);
                return BadRequest(new { error = $"Campaign {request.CampaignId} not found" });
            }

            if (campaign.TenantId != request.TenantId)
            {
                _logger.LogWarning(
                    "CreatePublishingJob: La campaña {CampaignId} pertenece al tenant {CampaignTenantId}, pero se intenta usar con tenant {RequestTenantId}",
                    request.CampaignId, campaign.TenantId, request.TenantId);
                return BadRequest(new { error = "Campaign does not belong to the specified tenant" });
            }

            if (!campaign.IsActive)
            {
                _logger.LogWarning(
                    "CreatePublishingJob: La campaña {CampaignId} no está activa",
                    request.CampaignId);
                return BadRequest(new { error = "Campaign is not active" });
            }

            // Construir payload si no se proporciona
            var payload = request.Payload;
            if (string.IsNullOrWhiteSpace(payload))
            {
                var payloadObj = new
                {
                    Copy = request.Content,
                    Hashtags = request.Hashtags,
                    MediaUrls = !string.IsNullOrWhiteSpace(request.MediaUrl) 
                        ? new List<string> { request.MediaUrl } 
                        : new List<string>(),
                    CampaignId = request.CampaignId.ToString(),
                    MarketingPackId = request.MarketingPackId?.ToString(),
                    GeneratedCopyId = request.GeneratedCopyId?.ToString(),
                    Metadata = new Dictionary<string, object>()
                };
                payload = JsonSerializer.Serialize(payloadObj);
            }

            // Crear PublishingJob
            var publishingJob = new PublishingJob
            {
                Id = request.Id ?? Guid.NewGuid(),
                TenantId = request.TenantId,
                CampaignId = request.CampaignId,
                MarketingPackId = request.MarketingPackId,
                GeneratedCopyId = request.GeneratedCopyId,
                Channel = request.Channel,
                Status = request.Status ?? "Success",
                ScheduledDate = request.ScheduledDate.HasValue 
                    ? DateTime.SpecifyKind(request.ScheduledDate.Value, DateTimeKind.Utc) 
                    : null,
                PublishedDate = request.PublishedDate.HasValue 
                    ? DateTime.SpecifyKind(request.PublishedDate.Value, DateTimeKind.Utc) 
                    : DateTime.UtcNow,
                PublishedUrl = request.PublishedUrl,
                ExternalPostId = request.ExternalPostId,
                Content = request.Content,
                Hashtags = request.Hashtags,
                MediaUrl = request.MediaUrl,
                ErrorMessage = request.ErrorMessage,
                Payload = payload,
                Metadata = request.Metadata,
                RequiresApproval = false, // Ya fue publicado, no requiere aprobación
                RetryCount = 0,
                MaxRetries = 3
            };

            await _publishingJobRepository.AddAsync(publishingJob, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PublishingJob {JobId} saved for Tenant {TenantId} on Channel {Channel} with status {Status}",
                publishingJob.Id,
                request.TenantId,
                request.Channel,
                publishingJob.Status);

            var response = new PublishingJobResponse
            {
                Id = publishingJob.Id,
                TenantId = publishingJob.TenantId,
                CampaignId = publishingJob.CampaignId,
                MarketingPackId = publishingJob.MarketingPackId,
                GeneratedCopyId = publishingJob.GeneratedCopyId,
                Channel = publishingJob.Channel,
                Status = publishingJob.Status,
                PublishedDate = publishingJob.PublishedDate,
                PublishedUrl = publishingJob.PublishedUrl,
                ExternalPostId = publishingJob.ExternalPostId,
                CreatedAt = publishingJob.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al guardar PublishingJob para Tenant {TenantId}",
                request.TenantId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Internal server error",
                    message = "Failed to save publishing job"
                });
        }
    }
}

/// <summary>
/// Request para crear PublishingJob.
/// </summary>
public class CreatePublishingJobRequest
{
    public Guid? Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid? MarketingPackId { get; set; }
    public Guid? GeneratedCopyId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ExternalPostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public string? MediaUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Payload { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Respuesta del endpoint de PublishingJob.
/// </summary>
public class PublishingJobResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid? MarketingPackId { get; set; }
    public Guid? GeneratedCopyId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PublishedDate { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ExternalPostId { get; set; }
    public DateTime CreatedAt { get; set; }
}

