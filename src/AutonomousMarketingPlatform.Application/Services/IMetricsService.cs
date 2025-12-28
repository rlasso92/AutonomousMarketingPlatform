using AutonomousMarketingPlatform.Application.DTOs;

namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para gestión de métricas de campañas y publicaciones.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Registra métricas de una campaña para una fecha específica.
    /// </summary>
    Task<CampaignMetricsDto> RegisterCampaignMetricsAsync(
        Guid tenantId,
        RegisterCampaignMetricsDto metrics,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra métricas de una publicación para una fecha específica.
    /// </summary>
    Task<PublishingJobMetricsDto> RegisterPublishingJobMetricsAsync(
        Guid tenantId,
        RegisterPublishingJobMetricsDto metrics,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas de una campaña en un rango de fechas.
    /// </summary>
    Task<List<CampaignMetricsDto>> GetCampaignMetricsAsync(
        Guid tenantId,
        Guid campaignId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas de una publicación en un rango de fechas.
    /// </summary>
    Task<List<PublishingJobMetricsDto>> GetPublishingJobMetricsAsync(
        Guid tenantId,
        Guid publishingJobId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene resumen de métricas de una campaña.
    /// </summary>
    Task<CampaignMetricsSummaryDto> GetCampaignMetricsSummaryAsync(
        Guid tenantId,
        Guid campaignId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene resumen de métricas de una publicación.
    /// </summary>
    Task<PublishingJobMetricsSummaryDto> GetPublishingJobMetricsSummaryAsync(
        Guid tenantId,
        Guid publishingJobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas de todas las campañas de un tenant.
    /// </summary>
    Task<List<CampaignMetricsSummaryDto>> GetAllCampaignsMetricsAsync(
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

