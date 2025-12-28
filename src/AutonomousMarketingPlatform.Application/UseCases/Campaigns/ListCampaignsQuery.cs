using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Campaigns;

/// <summary>
/// Query para listar campañas con filtros opcionales.
/// </summary>
public class ListCampaignsQuery : IRequest<List<CampaignListDto>>
{
    public Guid TenantId { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

/// <summary>
/// Handler para listar campañas.
/// </summary>
public class ListCampaignsQueryHandler : IRequestHandler<ListCampaignsQuery, List<CampaignListDto>>
{
    private readonly IRepository<Domain.Entities.Campaign> _campaignRepository;
    private readonly ILogger<ListCampaignsQueryHandler> _logger;

    public ListCampaignsQueryHandler(
        IRepository<Domain.Entities.Campaign> campaignRepository,
        ILogger<ListCampaignsQueryHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<List<CampaignListDto>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.FindAsync(
            c => c.TenantId == request.TenantId && 
                 c.IsActive &&
                 (string.IsNullOrEmpty(request.Status) || c.Status == request.Status),
            request.TenantId,
            cancellationToken);

        return campaigns
            .OrderByDescending(c => c.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new CampaignListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContentCount = c.Contents.Count,
                MarketingPackCount = c.MarketingPacks.Count,
                PublishingJobCount = c.PublishingJobs.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToList();
    }
}

