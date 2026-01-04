using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.AI;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

/// <summary>
/// Controlador API para gestión de MarketingPacks.
/// Usado por workflows n8n para guardar packs generados.
/// </summary>
/// <remarks>
/// NOTA DE SEGURIDAD: En producción, este endpoint debería tener autenticación por API key
/// o estar protegido por una red privada. Por ahora se permite acceso sin autenticación
/// para facilitar la integración con n8n en desarrollo.
/// </remarks>
[ApiController]
[Route("api/marketing-packs")]
[AllowAnonymous]
public class MarketingPacksApiController : ControllerBase
{
    private readonly IRepository<MarketingPack> _marketingPackRepository;
    private readonly IRepository<GeneratedCopy> _generatedCopyRepository;
    private readonly IRepository<MarketingAssetPrompt> _assetPromptRepository;
    private readonly IRepository<Content> _contentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MarketingPacksApiController> _logger;
    private readonly ILoggingService _loggingService;

    public MarketingPacksApiController(
        IRepository<MarketingPack> marketingPackRepository,
        IRepository<GeneratedCopy> generatedCopyRepository,
        IRepository<MarketingAssetPrompt> assetPromptRepository,
        IRepository<Content> contentRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext dbContext,
        ILogger<MarketingPacksApiController> logger,
        ILoggingService loggingService)
    {
        _marketingPackRepository = marketingPackRepository;
        _generatedCopyRepository = generatedCopyRepository;
        _assetPromptRepository = assetPromptRepository;
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _logger = logger;
        _loggingService = loggingService;
    }

    /// <summary>
    /// Obtiene un MarketingPack por ID o lista packs con filtros.
    /// Endpoint usado por workflows n8n para obtener packs existentes.
    /// </summary>
    /// <param name="id">ID del MarketingPack (opcional)</param>
    /// <param name="tenantId">ID del tenant (requerido si no se proporciona id)</param>
    /// <param name="orderBy">Campo para ordenar (opcional, ej: "cognitiveVersion")</param>
    /// <param name="limit">Límite de resultados (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>MarketingPack(s) encontrado(s)</returns>
    /// <response code="200">MarketingPack(s) encontrado(s)</response>
    /// <response code="400">Si los parámetros son inválidos</response>
    /// <response code="404">Si no se encuentra el pack (cuando se busca por ID)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(MarketingPackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<MarketingPackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketingPack(
        [FromQuery] Guid? id = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] string? orderBy = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Si se proporciona ID, buscar pack específico
            if (id.HasValue && id.Value != Guid.Empty)
            {
                var packs = await _marketingPackRepository.FindAsync(
                    p => p.Id == id.Value,
                    tenantId ?? Guid.Empty,
                    cancellationToken);
                
                var pack = packs.FirstOrDefault();
                if (pack == null)
                {
                    return NotFound(new { error = "MarketingPack not found" });
                }

                var response = new MarketingPackResponse
                {
                    Id = pack.Id,
                    TenantId = pack.TenantId,
                    UserId = pack.UserId,
                    ContentId = pack.ContentId,
                    CampaignId = pack.CampaignId,
                    Strategy = pack.Strategy,
                    Status = pack.Status,
                    Version = pack.Version,
                    Metadata = pack.Metadata,
                    CreatedAt = pack.CreatedAt
                };

                return Ok(response);
            }

            // Si no se proporciona ID, listar packs con filtros
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            {
                return BadRequest(new { error = "tenantId is required when id is not provided" });
            }

            var allPacks = await _marketingPackRepository.GetAllAsync(
                tenantId.Value,
                cancellationToken);

            // Aplicar ordenamiento
            IEnumerable<MarketingPack> orderedPacks = allPacks;
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                if (orderBy.Equals("cognitiveVersion", StringComparison.OrdinalIgnoreCase))
                {
                    // Extraer cognitiveVersion del Metadata si existe
                    orderedPacks = allPacks.OrderByDescending(p =>
                    {
                        if (p.Metadata != null && p.Metadata.Contains("cognitiveVersion"))
                        {
                            try
                            {
                                var parts = p.Metadata.Split(new[] { "\"cognitiveVersion\":" }, StringSplitOptions.None);
                                if (parts.Length > 1)
                                {
                                    var value = parts[1].Split(',')[0].Trim();
                                    if (int.TryParse(value, out int version))
                                        return version;
                                }
                            }
                            catch { }
                        }
                        return 0;
                    });
                }
                else if (orderBy.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                {
                    orderedPacks = allPacks.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                orderedPacks = allPacks.OrderByDescending(p => p.CreatedAt);
            }

            // Aplicar límite
            var results = limit.HasValue && limit.Value > 0
                ? orderedPacks.Take(limit.Value).ToList()
                : orderedPacks.ToList();
            var responses = results.Select(pack => new MarketingPackResponse
            {
                Id = pack.Id,
                TenantId = pack.TenantId,
                UserId = pack.UserId,
                ContentId = pack.ContentId,
                CampaignId = pack.CampaignId,
                Strategy = pack.Strategy,
                Status = pack.Status,
                Version = pack.Version,
                Metadata = pack.Metadata,
                CreatedAt = pack.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al obtener MarketingPack(s)");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Internal server error",
                    message = "Failed to get marketing pack(s)"
                });
        }
    }

    /// <summary>
    /// Crea o actualiza un MarketingPack desde n8n.
    /// Endpoint usado por workflows n8n para guardar packs generados.
    /// </summary>
    /// <param name="request">Datos del MarketingPack a guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>MarketingPack guardado</returns>
    /// <response code="200">MarketingPack guardado exitosamente</response>
    /// <response code="400">Si los datos son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(MarketingPackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrUpdateMarketingPack(
        [FromBody] CreateMarketingPackRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar datos requeridos
            if (request.TenantId == Guid.Empty)
            {
                _logger.LogWarning("CreateMarketingPack llamado con tenantId vacío");
                return BadRequest(new { error = "tenantId is required and must be a valid GUID" });
            }

            if (request.UserId == Guid.Empty)
            {
                _logger.LogWarning("CreateMarketingPack llamado con userId vacío");
                return BadRequest(new { error = "userId is required and must be a valid GUID" });
            }

            if (request.ContentId == Guid.Empty)
            {
                _logger.LogWarning("CreateMarketingPack llamado con contentId vacío");
                return BadRequest(new { error = "contentId is required and must be a valid GUID" });
            }

            if (string.IsNullOrWhiteSpace(request.Strategy))
            {
                return BadRequest(new { error = "strategy is required" });
            }

            // Verificar si el Content existe, si no, crearlo automáticamente
            // Usar DbContext directamente para evitar problemas de filtrado por tenant
            if (request.ContentId != Guid.Empty)
            {
                var existingContent = await _dbContext.Contents
                    .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);
                
                if (existingContent == null)
                {
                    // Crear Content automáticamente si no existe
                    var newContent = new Content
                    {
                        Id = request.ContentId,
                        TenantId = request.TenantId,
                        CampaignId = request.CampaignId,
                        ContentType = "Text", // Tipo por defecto para MarketingPack
                        FileUrl = $"auto-generated://marketing-pack/{request.ContentId}", // URL placeholder para contenido generado por IA
                        IsAiGenerated = true,
                        Description = "Content auto-created for MarketingPack"
                    };
                    _dbContext.Contents.Add(newContent);
                    // Guardar inmediatamente para que esté disponible para la foreign key
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Content {ContentId} auto-created for MarketingPack", request.ContentId);
                }
            }

            // Verificar si el pack ya existe (si se proporcionó ID)
            MarketingPack? existingPack = null;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                var existingPacks = await _marketingPackRepository.FindAsync(
                    p => p.Id == request.Id.Value,
                    request.TenantId,
                    cancellationToken);
                existingPack = existingPacks.FirstOrDefault();
            }

            MarketingPack marketingPack;
            if (existingPack != null)
            {
                // Actualizar pack existente
                existingPack.Strategy = request.Strategy;
                existingPack.Status = request.Status ?? "Generated";
                existingPack.Version = request.Version;
                existingPack.Metadata = request.Metadata;
                existingPack.UpdatedAt = DateTime.UtcNow;
                marketingPack = existingPack;
                await _marketingPackRepository.UpdateAsync(marketingPack, cancellationToken);
            }
            else
            {
                // Crear nuevo pack
                marketingPack = new MarketingPack
                {
                    Id = request.Id ?? Guid.NewGuid(),
                    TenantId = request.TenantId,
                    UserId = request.UserId,
                    ContentId = request.ContentId,
                    CampaignId = request.CampaignId,
                    Strategy = request.Strategy,
                    Status = request.Status ?? "Generated",
                    Version = request.Version,
                    Metadata = request.Metadata
                };
                await _marketingPackRepository.AddAsync(marketingPack, cancellationToken);
            }

            // Guardar GeneratedCopies si se proporcionaron
            if (request.Copies != null && request.Copies.Any())
            {
                foreach (var copyDto in request.Copies)
                {
                    var copy = new GeneratedCopy
                    {
                        Id = copyDto.Id ?? Guid.NewGuid(),
                        TenantId = request.TenantId,
                        MarketingPackId = marketingPack.Id,
                        CopyType = copyDto.CopyType,
                        Content = copyDto.Content,
                        Hashtags = copyDto.Hashtags,
                        SuggestedChannel = copyDto.SuggestedChannel,
                        PublicationChecklist = copyDto.PublicationChecklist != null
                            ? JsonSerializer.Serialize(copyDto.PublicationChecklist)
                            : null
                    };
                    await _generatedCopyRepository.AddAsync(copy, cancellationToken);
                }
            }

            // Guardar MarketingAssetPrompts si se proporcionaron
            if (request.AssetPrompts != null && request.AssetPrompts.Any())
            {
                foreach (var promptDto in request.AssetPrompts)
                {
                    var prompt = new MarketingAssetPrompt
                    {
                        Id = promptDto.Id ?? Guid.NewGuid(),
                        TenantId = request.TenantId,
                        MarketingPackId = marketingPack.Id,
                        AssetType = promptDto.AssetType,
                        Prompt = promptDto.Prompt,
                        NegativePrompt = promptDto.NegativePrompt,
                        Parameters = promptDto.Parameters != null
                            ? JsonSerializer.Serialize(promptDto.Parameters)
                            : null,
                        SuggestedChannel = promptDto.SuggestedChannel
                    };
                    await _assetPromptRepository.AddAsync(prompt, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MarketingPack {PackId} saved for Tenant {TenantId} with status {Status}",
                marketingPack.Id,
                request.TenantId,
                marketingPack.Status);

            var response = new MarketingPackResponse
            {
                Id = marketingPack.Id,
                TenantId = marketingPack.TenantId,
                UserId = marketingPack.UserId,
                ContentId = marketingPack.ContentId,
                CampaignId = marketingPack.CampaignId,
                Strategy = marketingPack.Strategy,
                Status = marketingPack.Status,
                Version = marketingPack.Version,
                Metadata = marketingPack.Metadata,
                CreatedAt = marketingPack.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Loggear en consola/archivo
            _logger.LogError(
                ex,
                "Error al guardar MarketingPack para Tenant {TenantId}. Tipo: {ExceptionType}, Mensaje: {Message}, InnerException: {InnerException}",
                request.TenantId,
                ex.GetType().Name,
                ex.Message,
                ex.InnerException?.Message ?? "N/A");

            // Si es DbUpdateException, loggear más detalles
            string? additionalData = null;
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError("DbUpdateException - Entries: {EntryCount}", dbEx.Entries?.Count ?? 0);
                var entryDetails = new List<object>();
                foreach (var entry in dbEx.Entries ?? Enumerable.Empty<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>())
                {
                    _logger.LogError("  Entry: {EntityType}, State: {State}", entry.Entity.GetType().Name, entry.State);
                    entryDetails.Add(new
                    {
                        EntityType = entry.Entity.GetType().Name,
                        State = entry.State.ToString(),
                        Properties = entry.Properties.Select(p => new
                        {
                            PropertyName = p.Metadata.Name,
                            CurrentValue = p.CurrentValue?.ToString(),
                            OriginalValue = p.OriginalValue?.ToString()
                        }).ToList()
                    });
                }
                additionalData = JsonSerializer.Serialize(new
                {
                    EntryCount = dbEx.Entries?.Count ?? 0,
                    Entries = entryDetails
                });
            }

            // Guardar error en la base de datos para validación posterior
            try
            {
                await _loggingService.LogErrorAsync(
                    message: $"Error al guardar MarketingPack. TenantId: {request.TenantId}, ContentId: {request.ContentId}",
                    source: "MarketingPacksApiController.CreateOrUpdateMarketingPack",
                    exception: ex,
                    tenantId: request.TenantId,
                    userId: request.UserId,
                    requestId: HttpContext.TraceIdentifier,
                    path: HttpContext.Request.Path,
                    httpMethod: HttpContext.Request.Method,
                    additionalData: additionalData ?? JsonSerializer.Serialize(new
                    {
                        RequestId = request.Id,
                        ContentId = request.ContentId,
                        CampaignId = request.CampaignId,
                        Status = request.Status,
                        CopiesCount = request.Copies?.Count ?? 0,
                        AssetPromptsCount = request.AssetPrompts?.Count ?? 0
                    }),
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: HttpContext.Request.Headers["User-Agent"].ToString());
            }
            catch (Exception logEx)
            {
                // Si falla al guardar el log en BD, solo loguear en consola
                _logger.LogError(logEx, "Error al guardar log en base de datos");
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Internal server error",
                    message = "Failed to save marketing pack",
                    details = ex.Message // Incluir detalles del error para debugging
                });
        }
    }
}

/// <summary>
/// Request para crear o actualizar MarketingPack.
/// </summary>
public class CreateMarketingPackRequest
{
    public Guid? Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public Guid? CampaignId { get; set; }
    public string Strategy { get; set; } = string.Empty;
    public string? Status { get; set; }
    public int Version { get; set; } = 1;
    public string? Metadata { get; set; }
    public List<GeneratedCopyRequest>? Copies { get; set; }
    public List<MarketingAssetPromptRequest>? AssetPrompts { get; set; }
}

/// <summary>
/// Request para GeneratedCopy.
/// </summary>
public class GeneratedCopyRequest
{
    public Guid? Id { get; set; }
    public string CopyType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Hashtags { get; set; }
    public string? SuggestedChannel { get; set; }
    public Dictionary<string, object>? PublicationChecklist { get; set; }
}

/// <summary>
/// Request para MarketingAssetPrompt.
/// </summary>
public class MarketingAssetPromptRequest
{
    public Guid? Id { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string? SuggestedChannel { get; set; }
}

/// <summary>
/// Respuesta del endpoint de MarketingPack.
/// </summary>
public class MarketingPackResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public Guid? CampaignId { get; set; }
    public string Strategy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

