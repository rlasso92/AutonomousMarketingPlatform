using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.Metrics;

/// <summary>
/// Query para obtener métricas de una publicación.
/// </summary>
public class GetPublishingJobMetricsQuery : IRequest<PublishingJobMetricsSummaryDto>
{
    public Guid TenantId { get; set; }
    public Guid PublishingJobId { get; set; }
}

/// <summary>
/// Handler para obtener métricas de publicación.
/// </summary>
public class GetPublishingJobMetricsQueryHandler : IRequestHandler<GetPublishingJobMetricsQuery, PublishingJobMetricsSummaryDto>
{
    private readonly IMetricsService _metricsService;

    public GetPublishingJobMetricsQueryHandler(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public async Task<PublishingJobMetricsSummaryDto> Handle(GetPublishingJobMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _metricsService.GetPublishingJobMetricsSummaryAsync(
            request.TenantId,
            request.PublishingJobId,
            cancellationToken);
    }
}

