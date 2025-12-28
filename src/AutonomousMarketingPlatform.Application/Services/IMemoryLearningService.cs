namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para aprendizaje automático basado en métricas.
/// Analiza métricas y actualiza MarketingMemory con aprendizajes.
/// </summary>
public interface IMemoryLearningService
{
    /// <summary>
    /// Analiza métricas de una campaña y actualiza la memoria con aprendizajes.
    /// </summary>
    Task LearnFromCampaignMetricsAsync(
        Guid tenantId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analiza métricas de una publicación y actualiza la memoria con aprendizajes.
    /// </summary>
    Task LearnFromPublishingJobMetricsAsync(
        Guid tenantId,
        Guid publishingJobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Proceso automático que analiza todas las métricas recientes y actualiza la memoria.
    /// Debe ejecutarse periódicamente (ej: diariamente).
    /// </summary>
    Task ProcessLearningFromRecentMetricsAsync(
        Guid tenantId,
        int daysToAnalyze = 7,
        CancellationToken cancellationToken = default);
}

