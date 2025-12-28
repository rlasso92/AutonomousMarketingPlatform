using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.Metrics;

/// <summary>
/// Query para obtener métricas de una campaña.
/// </summary>
public class GetCampaignMetricsQuery : IRequest<CampaignMetricsSummaryDto>
{
    public Guid TenantId { get; set; }
    public Guid CampaignId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Handler para obtener métricas de campaña.
/// </summary>
public class GetCampaignMetricsQueryHandler : IRequestHandler<GetCampaignMetricsQuery, CampaignMetricsSummaryDto>
{
    private readonly IMetricsService _metricsService;

    public GetCampaignMetricsQueryHandler(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public async Task<CampaignMetricsSummaryDto> Handle(GetCampaignMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _metricsService.GetCampaignMetricsSummaryAsync(
            request.TenantId,
            request.CampaignId,
            request.FromDate,
            request.ToDate,
            cancellationToken);
    }
}

