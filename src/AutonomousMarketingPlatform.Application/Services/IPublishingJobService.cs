namespace AutonomousMarketingPlatform.Application.Services;

/// <summary>
/// Servicio para gestionar trabajos de publicación.
/// </summary>
public interface IPublishingJobService
{
    /// <summary>
    /// Procesa un trabajo de publicación.
    /// </summary>
    Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene trabajos pendientes para procesar.
    /// </summary>
    Task<List<Guid>> GetPendingJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene trabajos programados que deben ejecutarse ahora.
    /// </summary>
    Task<List<Guid>> GetScheduledJobsAsync(CancellationToken cancellationToken = default);
}

