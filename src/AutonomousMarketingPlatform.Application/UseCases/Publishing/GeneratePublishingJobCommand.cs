using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Application.UseCases.Publishing;

/// <summary>
/// Comando para generar un trabajo de publicación desde un MarketingPack.
/// </summary>
public class GeneratePublishingJobCommand : IRequest<PublishingJobDto>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid MarketingPackId { get; set; }
    public Guid? GeneratedCopyId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool RequiresApproval { get; set; } = true;
}

/// <summary>
/// Handler para generar trabajo de publicación.
/// </summary>
public class GeneratePublishingJobCommandHandler : IRequestHandler<GeneratePublishingJobCommand, PublishingJobDto>
{
    private readonly IRepository<PublishingJob> _publishingJobRepository;
    private readonly IRepository<MarketingPack> _marketingPackRepository;
    private readonly IRepository<GeneratedCopy> _copyRepository;
    private readonly IRepository<Domain.Entities.Content> _contentRepository;
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GeneratePublishingJobCommandHandler> _logger;

    public GeneratePublishingJobCommandHandler(
        IRepository<PublishingJob> publishingJobRepository,
        IRepository<MarketingPack> marketingPackRepository,
        IRepository<GeneratedCopy> copyRepository,
        IRepository<Domain.Entities.Content> contentRepository,
        IRepository<Campaign> campaignRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<GeneratePublishingJobCommandHandler> logger)
    {
        _publishingJobRepository = publishingJobRepository;
        _marketingPackRepository = marketingPackRepository;
        _copyRepository = copyRepository;
        _contentRepository = contentRepository;
        _campaignRepository = campaignRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PublishingJobDto> Handle(GeneratePublishingJobCommand request, CancellationToken cancellationToken)
    {
        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Validar que la campaña existe y pertenece al tenant correcto
        var campaign = await _campaignRepository.GetByIdAsync(
            request.CampaignId, request.TenantId, cancellationToken);
        
        if (campaign == null)
        {
            throw new NotFoundException($"Campaña {request.CampaignId} no encontrada");
        }

        if (campaign.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException(
                $"La campaña {request.CampaignId} no pertenece al tenant {request.TenantId}");
        }

        if (!campaign.IsActive)
        {
            throw new InvalidOperationException(
                $"La campaña {request.CampaignId} no está activa");
        }

        // Validar que el canal especificado esté en los canales objetivo de la campaña
        if (!string.IsNullOrWhiteSpace(request.Channel))
        {
            var campaignChannels = campaign.TargetChannels != null
                ? JsonSerializer.Deserialize<List<string>>(campaign.TargetChannels) ?? new List<string>()
                : new List<string>();

            // Normalizar nombres de canales para comparación (case-insensitive)
            var normalizedCampaignChannels = campaignChannels
                .Select(c => c.ToLowerInvariant().Trim())
                .ToList();

            var normalizedRequestChannel = request.Channel.ToLowerInvariant().Trim();

            // Mapeo de variaciones comunes de nombres de canales
            var channelMappings = new Dictionary<string, List<string>>
            {
                { "instagram", new List<string> { "instagram", "ig" } },
                { "facebook", new List<string> { "facebook", "fb" } },
                { "tiktok", new List<string> { "tiktok", "tt" } },
                { "twitter", new List<string> { "twitter", "x" } },
                { "linkedin", new List<string> { "linkedin", "li" } }
            };

            // Verificar si el canal está en la lista o en alguna de sus variaciones
            var channelFound = normalizedCampaignChannels.Contains(normalizedRequestChannel) ||
                normalizedCampaignChannels.Any(cc => 
                    channelMappings.ContainsKey(cc) && channelMappings[cc].Contains(normalizedRequestChannel)) ||
                channelMappings.Any(kvp => 
                    kvp.Value.Contains(normalizedRequestChannel) && 
                    normalizedCampaignChannels.Contains(kvp.Key));

            if (!channelFound && campaignChannels.Count > 0)
            {
                _logger.LogWarning(
                    "Canal '{Channel}' no está en los canales objetivo de la campaña {CampaignId}. Canales permitidos: {AllowedChannels}",
                    request.Channel, request.CampaignId, string.Join(", ", campaignChannels));
                
                throw new InvalidOperationException(
                    $"El canal '{request.Channel}' no está en los canales objetivo de la campaña. " +
                    $"Canales permitidos: {string.Join(", ", campaignChannels)}");
            }
        }

        // Obtener MarketingPack
        var marketingPack = await _marketingPackRepository.GetByIdAsync(
            request.MarketingPackId, request.TenantId, cancellationToken);
        
        if (marketingPack == null)
        {
            throw new NotFoundException($"MarketingPack {request.MarketingPackId} no encontrado");
        }

        // Validar que el MarketingPack pertenece al tenant correcto
        if (marketingPack.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException(
                $"El MarketingPack {request.MarketingPackId} no pertenece al tenant {request.TenantId}");
        }

        // Si el MarketingPack tiene un CampaignId, debe coincidir con el CampaignId del request
        if (marketingPack.CampaignId.HasValue && marketingPack.CampaignId.Value != request.CampaignId)
        {
            throw new InvalidOperationException(
                $"El MarketingPack {request.MarketingPackId} está asociado a la campaña {marketingPack.CampaignId.Value}, " +
                $"pero se intenta usar con la campaña {request.CampaignId}");
        }

        // Obtener Copy (si se especificó)
        GeneratedCopy? copy = null;
        if (request.GeneratedCopyId.HasValue)
        {
            var copies = await _copyRepository.FindAsync(
                c => c.Id == request.GeneratedCopyId.Value && c.MarketingPackId == request.MarketingPackId,
                request.TenantId,
                cancellationToken);
            copy = copies.FirstOrDefault();
        }
        else
        {
            // Usar el copy recomendado para el canal
            var copies = await _copyRepository.FindAsync(
                c => c.MarketingPackId == request.MarketingPackId,
                request.TenantId,
                cancellationToken);
            
            copy = request.Channel switch
            {
                "Instagram" => copies.FirstOrDefault(c => c.CopyType == "Short" || c.SuggestedChannel == "Instagram"),
                "Facebook" => copies.FirstOrDefault(c => c.CopyType == "Medium" || c.SuggestedChannel == "Facebook"),
                "TikTok" => copies.FirstOrDefault(c => c.CopyType == "Long" || c.SuggestedChannel == "TikTok"),
                _ => copies.FirstOrDefault()
            };
        }

        if (copy == null)
        {
            throw new NotFoundException("No se encontró un copy adecuado para la publicación");
        }

        // Obtener contenido asociado
        Domain.Entities.Content? content = await _contentRepository.GetByIdAsync(
            marketingPack.ContentId, request.TenantId, cancellationToken);

        // Construir payload
        var payload = new Domain.Interfaces.PublishingPayload
        {
            Copy = copy.Content,
            Hashtags = copy.Hashtags,
            MediaUrls = content != null ? new List<string> { content.FileUrl } : new List<string>(),
            CampaignId = request.CampaignId.ToString(),
            MarketingPackId = request.MarketingPackId.ToString(),
            Metadata = new Dictionary<string, object>
            {
                { "MarketingPackId", request.MarketingPackId },
                { "GeneratedCopyId", copy.Id },
                { "ContentId", marketingPack.ContentId }
            }
        };

        // Crear PublishingJob
        var job = new PublishingJob
        {
            TenantId = request.TenantId,
            CampaignId = request.CampaignId,
            MarketingPackId = request.MarketingPackId,
            GeneratedCopyId = copy.Id,
            Channel = request.Channel,
            Status = "Pending",
            ScheduledDate = request.ScheduledDate,
            Content = copy.Content,
            Hashtags = copy.Hashtags,
            MediaUrl = content?.FileUrl,
            Payload = JsonSerializer.Serialize(payload),
            RequiresApproval = request.RequiresApproval,
            MaxRetries = 3
        };

        await _publishingJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("PublishingJob generado: {JobId} para canal {Channel} en campaña {CampaignId}",
            job.Id, request.Channel, request.CampaignId);

        // Auditoría
        await _auditService.LogAsync(
            request.TenantId,
            "GeneratePublishingJob",
            "PublishingJob",
            job.Id,
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

        // Retornar DTO
        return new PublishingJobDto
        {
            Id = job.Id,
            CampaignId = job.CampaignId,
            MarketingPackId = job.MarketingPackId,
            GeneratedCopyId = job.GeneratedCopyId,
            Channel = job.Channel,
            Status = job.Status,
            ScheduledDate = job.ScheduledDate,
            PublishedDate = job.PublishedDate,
            PublishedUrl = job.PublishedUrl,
            Content = job.Content,
            Hashtags = job.Hashtags,
            MediaUrl = job.MediaUrl,
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            RequiresApproval = job.RequiresApproval,
            DownloadUrl = job.DownloadUrl,
            CreatedAt = job.CreatedAt
        };
    }
}

/// <summary>
/// Excepción para recursos no encontrados.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

