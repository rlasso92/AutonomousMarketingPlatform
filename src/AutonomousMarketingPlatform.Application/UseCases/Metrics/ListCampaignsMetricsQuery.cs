using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.Metrics;

/// <summary>
/// Query para listar métricas de todas las campañas.
/// </summary>
public class ListCampaignsMetricsQuery : IRequest<List<CampaignMetricsSummaryDto>>
{
    public Guid TenantId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Handler para listar métricas de campañas.
/// </summary>
public class ListCampaignsMetricsQueryHandler : IRequestHandler<ListCampaignsMetricsQuery, List<CampaignMetricsSummaryDto>>
{
    private readonly IMetricsService _metricsService;

    public ListCampaignsMetricsQueryHandler(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public async Task<List<CampaignMetricsSummaryDto>> Handle(ListCampaignsMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _metricsService.GetAllCampaignsMetricsAsync(
            request.TenantId,
            request.FromDate,
            request.ToDate,
            cancellationToken);
    }
}

