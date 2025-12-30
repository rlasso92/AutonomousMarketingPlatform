using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.Validators;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Application.UseCases.Campaigns;

/// <summary>
/// Comando para crear una nueva campaña.
/// </summary>
public class CreateCampaignCommand : IRequest<CampaignDetailDto>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public CreateCampaignDto Campaign { get; set; } = null!;
}

/// <summary>
/// Handler para crear campaña.
/// </summary>
public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignDetailDto>
{
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCampaignDto> _validator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateCampaignCommandHandler> _logger;

    public CreateCampaignCommandHandler(
        IRepository<Campaign> campaignRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IValidator<CreateCampaignDto> validator,
        UserManager<ApplicationUser> userManager,
        ILogger<CreateCampaignCommandHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<CampaignDetailDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("=== HANDLER EJECUTADO - LLEGÓ AQUÍ ===");
        _logger.LogInformation("========================================");
        _logger.LogInformation("=== INICIO CreateCampaignCommandHandler ===");
        _logger.LogInformation("Request: TenantId={TenantId}, UserId={UserId}, CampaignName={CampaignName}", 
            request.TenantId, request.UserId, request.Campaign?.Name ?? "NULL");
        
        if (request == null)
        {
            _logger.LogError("ERROR: request es NULL!");
            throw new ArgumentNullException(nameof(request));
        }
        
        if (request.Campaign == null)
        {
            _logger.LogError("ERROR: request.Campaign es NULL!");
            throw new ArgumentNullException(nameof(request.Campaign));
        }

        // Validar DTO
        _logger.LogInformation("Validando DTO de campaña...");
        var validationResult = await _validator.ValidateAsync(request.Campaign, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validación falló. Errores: {ErrorCount}", validationResult.Errors.Count);
            foreach (var error in validationResult.Errors)
            {
                _logger.LogError("  - {PropertyName}: {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            throw new ValidationException(validationResult.Errors);
        }
        _logger.LogInformation("Validación exitosa");

        // Validar usuario pertenece al tenant (excepto para SuperAdmin)
        _logger.LogInformation("Validando que usuario {UserId} pertenece al tenant {TenantId}...", 
            request.UserId, request.TenantId);
        
        // Obtener el usuario para verificar si es SuperAdmin
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            _logger.LogError("Usuario {UserId} no encontrado", request.UserId);
            throw new UnauthorizedAccessException("Usuario no encontrado");
        }
        
        // SuperAdmin tiene TenantId == Guid.Empty y puede crear campañas para cualquier tenant
        var isSuperAdmin = user.TenantId == Guid.Empty;
        
        if (isSuperAdmin)
        {
            _logger.LogInformation("Usuario {UserId} es SuperAdmin (TenantId={UserTenantId}), permitiendo crear campaña para tenant {TenantId}", 
                request.UserId, user.TenantId, request.TenantId);
        }
        else
        {
            // Para usuarios normales, validar que pertenecen al tenant
            var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
                request.UserId, request.TenantId, cancellationToken);
            
            if (!userBelongsToTenant)
            {
                _logger.LogError("Usuario {UserId} NO pertenece al tenant {TenantId}", request.UserId, request.TenantId);
                throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
            }
            _logger.LogInformation("Validación de usuario exitosa");
        }

        // Crear campaña
        _logger.LogInformation("Creando entidad Campaign...");
        _logger.LogInformation("Objectives recibido: {Objectives}", 
            request.Campaign.Objectives != null ? JsonSerializer.Serialize(request.Campaign.Objectives) : "NULL");
        _logger.LogInformation("TargetAudience recibido: {TargetAudience}", 
            request.Campaign.TargetAudience != null ? JsonSerializer.Serialize(request.Campaign.TargetAudience) : "NULL");
        _logger.LogInformation("TargetChannels recibido: {TargetChannels}", 
            request.Campaign.TargetChannels != null ? JsonSerializer.Serialize(request.Campaign.TargetChannels) : "NULL");
        
        // Asegurar que Objectives, TargetAudience y TargetChannels nunca sean null (usar JSON vacío si es necesario)
        var objectivesJson = request.Campaign.Objectives != null && request.Campaign.Objectives.Count > 0
            ? JsonSerializer.Serialize(request.Campaign.Objectives)
            : "{}"; // JSON vacío en lugar de null
            
        var targetAudienceJson = request.Campaign.TargetAudience != null && request.Campaign.TargetAudience.Count > 0
            ? JsonSerializer.Serialize(request.Campaign.TargetAudience)
            : "{}"; // JSON vacío en lugar de null
            
        var targetChannelsJson = request.Campaign.TargetChannels != null && request.Campaign.TargetChannels.Count > 0
            ? JsonSerializer.Serialize(request.Campaign.TargetChannels)
            : "[]"; // JSON array vacío en lugar de null
        
        _logger.LogInformation("JSONs preparados - Objectives: {Objectives}, TargetAudience: {TargetAudience}, TargetChannels: {TargetChannels}", 
            objectivesJson, targetAudienceJson, targetChannelsJson);
        
        // Normalizar fechas a UTC (PostgreSQL requiere UTC para timestamp with time zone)
        var startDateUtc = request.Campaign.StartDate.HasValue 
            ? NormalizeToUtc(request.Campaign.StartDate.Value) 
            : (DateTime?)null;
        var endDateUtc = request.Campaign.EndDate.HasValue 
            ? NormalizeToUtc(request.Campaign.EndDate.Value) 
            : (DateTime?)null;
        
        _logger.LogInformation("Fechas normalizadas a UTC:");
        _logger.LogInformation("  StartDate: {Original} -> {Utc} (Kind: {Kind})", 
            request.Campaign.StartDate, startDateUtc, startDateUtc?.Kind);
        _logger.LogInformation("  EndDate: {Original} -> {Utc} (Kind: {Kind})", 
            request.Campaign.EndDate, endDateUtc, endDateUtc?.Kind);
        
        var campaign = new Campaign
        {
            TenantId = request.TenantId,
            Name = request.Campaign.Name,
            Description = request.Campaign.Description,
            Status = request.Campaign.Status,
            StartDate = startDateUtc,
            EndDate = endDateUtc,
            Budget = request.Campaign.Budget,
            Objectives = objectivesJson,
            TargetAudience = targetAudienceJson,
            TargetChannels = targetChannelsJson,
            Notes = request.Campaign.Notes,
            // CreatedAt ya se inicializa en BaseEntity como DateTime.UtcNow, pero lo aseguramos aquí
            CreatedAt = DateTime.UtcNow
        };
        _logger.LogInformation("Entidad Campaign creada en memoria. Id={CampaignId}, Name={Name}, TenantId={TenantId}", 
            campaign.Id, campaign.Name, campaign.TenantId);
        _logger.LogInformation("Valores finales de la entidad:");
        _logger.LogInformation("  Objectives = '{Objectives}' (Length: {Length}, IsNull: {IsNull})", 
            campaign.Objectives, campaign.Objectives?.Length ?? 0, campaign.Objectives == null);
        _logger.LogInformation("  TargetAudience = '{TargetAudience}' (Length: {Length}, IsNull: {IsNull})", 
            campaign.TargetAudience, campaign.TargetAudience?.Length ?? 0, campaign.TargetAudience == null);
        _logger.LogInformation("  TargetChannels = '{TargetChannels}' (Length: {Length}, IsNull: {IsNull})", 
            campaign.TargetChannels, campaign.TargetChannels?.Length ?? 0, campaign.TargetChannels == null);

        _logger.LogInformation("Agregando campaña al repositorio...");
        try
        {
            await _campaignRepository.AddAsync(campaign, cancellationToken);
            _logger.LogInformation("Campaña agregada al repositorio. CampaignId={CampaignId}", campaign.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar campaña al repositorio");
            throw;
        }

        _logger.LogInformation("Guardando cambios en la base de datos...");
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cambios guardados exitosamente en BD. CampaignId={CampaignId}", campaign.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("=== ERROR AL GUARDAR EN BASE DE DATOS ===");
            _logger.LogError("Tipo de excepción: {ExceptionType}", ex.GetType().Name);
            _logger.LogError("Mensaje: {Message}", ex.Message);
            _logger.LogError("Inner Exception: {InnerException}", ex.InnerException?.Message ?? "N/A");
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            
            // Si es DbUpdateException, loggear más detalles
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError("DbUpdateException - Entries: {EntryCount}", dbEx.Entries?.Count ?? 0);
                foreach (var entry in dbEx.Entries ?? Enumerable.Empty<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>())
                {
                    _logger.LogError("  Entry: {EntityType}, State: {State}", entry.Entity.GetType().Name, entry.State);
                    foreach (var prop in entry.Properties)
                    {
                        _logger.LogError("    {PropertyName} = {CurrentValue} (Original: {OriginalValue})", 
                            prop.Metadata.Name, prop.CurrentValue, prop.OriginalValue);
                    }
                }
            }
            
            throw;
        }

        _logger.LogInformation("Campaña creada exitosamente: CampaignId={CampaignId} por usuario {UserId} en tenant {TenantId}", 
            campaign.Id, request.UserId, request.TenantId);

        // Auditoría
        _logger.LogInformation("Registrando auditoría...");
        await _auditService.LogAsync(
            request.TenantId,
            "CreateCampaign",
            "Campaign",
            campaign.Id,
            request.UserId,
            null,
            null,
            null,
            null,
            "Success",
            null,
            null,
            null,
            cancellationToken);
        _logger.LogInformation("Auditoría registrada");

        // Retornar DTO de detalle (las relaciones se cargarán lazy si están disponibles)
        _logger.LogInformation("Mapeando a CampaignDetailDto...");
        var result = MapToDetailDto(campaign);
        _logger.LogInformation("=== FIN CreateCampaignCommandHandler - EXITOSO ===");
        return result;
    }

    /// <summary>
    /// Normaliza un DateTime a UTC para PostgreSQL (timestamp with time zone).
    /// PostgreSQL solo acepta DateTime con Kind=Utc.
    /// </summary>
    private static DateTime NormalizeToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };
    }

    private CampaignDetailDto MapToDetailDto(Campaign campaign)
    {
        return new CampaignDetailDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            Status = campaign.Status,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            Objectives = campaign.Objectives != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(campaign.Objectives)
                : null,
            TargetAudience = campaign.TargetAudience != null
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(campaign.TargetAudience)
                : null,
            TargetChannels = campaign.TargetChannels != null
                ? JsonSerializer.Deserialize<List<string>>(campaign.TargetChannels)
                : null,
            Notes = campaign.Notes,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            Contents = campaign.Contents.Select(c => new ContentListItemDto
            {
                Id = c.Id,
                ContentType = c.ContentType,
                Description = c.Description,
                FileUrl = c.FileUrl,
                CreatedAt = c.CreatedAt
            }).ToList(),
            MarketingPacks = campaign.MarketingPacks.Select(mp => new MarketingPackListItemDto
            {
                Id = mp.Id,
                Status = mp.Status,
                Version = mp.Version,
                CopyCount = mp.Copies.Count,
                AssetPromptCount = mp.AssetPrompts.Count,
                CreatedAt = mp.CreatedAt
            }).ToList(),
            PublishingJobs = campaign.PublishingJobs.Select(pj => new PublishingJobListItemDto
            {
                Id = pj.Id,
                Channel = pj.Channel,
                Status = pj.Status,
                ScheduledDate = pj.ScheduledDate,
                PublishedDate = pj.PublishedDate,
                PublishedUrl = pj.PublishedUrl,
                ErrorMessage = pj.ErrorMessage,
                CreatedAt = pj.CreatedAt
            }).ToList()
        };
    }
}

