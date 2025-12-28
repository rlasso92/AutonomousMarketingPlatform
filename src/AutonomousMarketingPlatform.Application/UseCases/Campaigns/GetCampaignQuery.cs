using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Application.UseCases.Campaigns;

/// <summary>
/// Query para obtener una campaña por ID.
/// </summary>
public class GetCampaignQuery : IRequest<CampaignDetailDto?>
{
    public Guid TenantId { get; set; }
    public Guid CampaignId { get; set; }
}

/// <summary>
/// Handler para obtener campaña.
/// </summary>
public class GetCampaignQueryHandler : IRequestHandler<GetCampaignQuery, CampaignDetailDto?>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<GetCampaignQueryHandler> _logger;

    public GetCampaignQueryHandler(
        ICampaignRepository campaignRepository,
        ILogger<GetCampaignQueryHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<CampaignDetailDto?> Handle(GetCampaignQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetCampaignWithDetailsAsync(request.CampaignId, request.TenantId, cancellationToken);
        
        if (campaign == null || !campaign.IsActive)
        {
            return null;
        }

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

