using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.Validators;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using FluentValidation;
using MediatR;
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
    private readonly ILogger<CreateCampaignCommandHandler> _logger;

    public CreateCampaignCommandHandler(
        IRepository<Campaign> campaignRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IValidator<CreateCampaignDto> validator,
        ILogger<CreateCampaignCommandHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CampaignDetailDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        // Validar DTO
        var validationResult = await _validator.ValidateAsync(request.Campaign, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Crear campaña
        var campaign = new Campaign
        {
            TenantId = request.TenantId,
            Name = request.Campaign.Name,
            Description = request.Campaign.Description,
            Status = request.Campaign.Status,
            StartDate = request.Campaign.StartDate,
            EndDate = request.Campaign.EndDate,
            Budget = request.Campaign.Budget,
            Objectives = request.Campaign.Objectives != null
                ? JsonSerializer.Serialize(request.Campaign.Objectives)
                : null,
            TargetAudience = request.Campaign.TargetAudience != null
                ? JsonSerializer.Serialize(request.Campaign.TargetAudience)
                : null,
            TargetChannels = request.Campaign.TargetChannels != null
                ? JsonSerializer.Serialize(request.Campaign.TargetChannels)
                : null,
            Notes = request.Campaign.Notes
        };

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaña creada: {CampaignId} por usuario {UserId} en tenant {TenantId}", 
            campaign.Id, request.UserId, request.TenantId);

        // Auditoría
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

        // Retornar DTO de detalle (las relaciones se cargarán lazy si están disponibles)
        return MapToDetailDto(campaign);
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

