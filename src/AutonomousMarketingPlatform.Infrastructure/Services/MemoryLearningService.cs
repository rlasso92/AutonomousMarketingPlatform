using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Servicio para aprendizaje automático basado en métricas.
/// Analiza métricas y actualiza MarketingMemory con aprendizajes.
/// </summary>
public class MemoryLearningService : IMemoryLearningService
{
    private readonly IMetricsService _metricsService;
    private readonly IMarketingMemoryService _memoryService;
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<PublishingJob> _publishingJobRepository;
    private readonly IRepository<GeneratedCopy> _copyRepository;
    private readonly ILogger<MemoryLearningService> _logger;

    public MemoryLearningService(
        IMetricsService metricsService,
        IMarketingMemoryService memoryService,
        IRepository<Campaign> campaignRepository,
        IRepository<PublishingJob> publishingJobRepository,
        IRepository<GeneratedCopy> copyRepository,
        ILogger<MemoryLearningService> logger)
    {
        _metricsService = metricsService;
        _memoryService = memoryService;
        _campaignRepository = campaignRepository;
        _publishingJobRepository = publishingJobRepository;
        _copyRepository = copyRepository;
        _logger = logger;
    }

    public async Task LearnFromCampaignMetricsAsync(
        Guid tenantId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _metricsService.GetCampaignMetricsSummaryAsync(
                tenantId, campaignId, null, null, cancellationToken);

            if (summary.DaysWithMetrics == 0)
            {
                _logger.LogInformation("No hay métricas para aprender de la campaña {CampaignId}", campaignId);
                return;
            }

            var campaign = await _campaignRepository.GetByIdAsync(campaignId, tenantId, cancellationToken);
            if (campaign == null) return;

            // Determinar si la campaña fue exitosa
            var isSuccessful = summary.AverageEngagementRate.HasValue && 
                              summary.AverageEngagementRate.Value > 3.0m; // 3% engagement rate como umbral

            // Generar aprendizajes
            var learnings = new List<string>();

            // Aprendizaje sobre engagement
            if (summary.AverageEngagementRate.HasValue)
            {
                if (summary.AverageEngagementRate.Value > 5.0m)
                {
                    learnings.Add($"Campaña '{campaign.Name}' tuvo excelente engagement ({summary.AverageEngagementRate.Value:F2}%). " +
                                 $"Total: {summary.TotalEngagement} interacciones ({summary.TotalLikes} likes, {summary.TotalComments} comentarios, {summary.TotalShares} compartidos).");
                }
                else if (summary.AverageEngagementRate.Value < 1.0m)
                {
                    learnings.Add($"Campaña '{campaign.Name}' tuvo bajo engagement ({summary.AverageEngagementRate.Value:F2}%). " +
                                 $"Revisar estrategia, contenido o audiencia objetivo.");
                }
            }

            // Aprendizaje sobre CTR
            if (summary.AverageClickThroughRate.HasValue)
            {
                if (summary.AverageClickThroughRate.Value > 2.0m)
                {
                    learnings.Add($"Campaña '{campaign.Name}' tuvo buen CTR ({summary.AverageClickThroughRate.Value:F2}%). " +
                                 $"El contenido generó {summary.TotalClicks} clics de {summary.TotalImpressions} impresiones.");
                }
            }

            // Aprendizaje sobre canales (si hay múltiples publicaciones)
            var publishingJobs = await _publishingJobRepository.FindAsync(
                p => p.CampaignId == campaignId && p.TenantId == tenantId,
                tenantId,
                cancellationToken);

            var channelPerformance = publishingJobs
                .Where(p => p.Status == "Success")
                .GroupBy(p => p.Channel)
                .Select(g => new { Channel = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (channelPerformance.Any())
            {
                var topChannel = channelPerformance.First();
                learnings.Add($"Canal más utilizado en campaña '{campaign.Name}': {topChannel.Channel} ({topChannel.Count} publicaciones).");
            }

            // Guardar aprendizajes en memoria
            foreach (var learning in learnings)
            {
                var tags = new List<string> { "Metrics", "Campaign", campaign.Name };
                if (isSuccessful) tags.Add("Success");
                else tags.Add("NeedsImprovement");

                var context = new Dictionary<string, object>
                {
                    { "CampaignId", campaignId },
                    { "CampaignName", campaign.Name },
                    { "TotalImpressions", summary.TotalImpressions },
                    { "TotalEngagement", summary.TotalEngagement },
                    { "AverageEngagementRate", summary.AverageEngagementRate },
                    { "AverageClickThroughRate", summary.AverageClickThroughRate },
                    { "IsSuccessful", isSuccessful }
                };

                await _memoryService.SaveLearningMemoryAsync(
                    tenantId,
                    learning,
                    tags,
                    isSuccessful ? 8 : 6, // Mayor relevancia si fue exitoso
                    cancellationToken);

                _logger.LogInformation("Aprendizaje guardado en memoria: {Learning}", learning);
            }

            // Guardar preferencias detectadas
            if (isSuccessful && campaign.MarketingStrategy != null)
            {
                var preferenceContent = $"Estrategia exitosa en campaña '{campaign.Name}': {campaign.MarketingStrategy.Substring(0, Math.Min(200, campaign.MarketingStrategy.Length))}";
                
                await _memoryService.SaveLearningMemoryAsync(
                    tenantId,
                    preferenceContent,
                    new List<string> { "Preference", "Strategy", "Successful" },
                    7,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprender de métricas de campaña {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task LearnFromPublishingJobMetricsAsync(
        Guid tenantId,
        Guid publishingJobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _metricsService.GetPublishingJobMetricsSummaryAsync(
                tenantId, publishingJobId, cancellationToken);

            if (summary.DaysWithMetrics == 0)
            {
                return;
            }

            var publishingJob = await _publishingJobRepository.GetByIdAsync(
                publishingJobId, tenantId, cancellationToken);
            if (publishingJob == null) return;

            // Determinar si el post fue exitoso
            var isSuccessful = summary.AverageEngagementRate.HasValue && 
                              summary.AverageEngagementRate.Value > 3.0m;

            var learnings = new List<string>();

            // Aprendizaje sobre el contenido
            if (isSuccessful && !string.IsNullOrEmpty(publishingJob.Content))
            {
                var contentPreview = publishingJob.Content.Length > 100 
                    ? publishingJob.Content.Substring(0, 100) + "..." 
                    : publishingJob.Content;

                learnings.Add($"Post exitoso en {publishingJob.Channel}: '{contentPreview}' " +
                            $"obtuvo {summary.TotalEngagement} interacciones ({summary.AverageEngagementRate:F2}% engagement rate). " +
                            $"Contenido similar podría funcionar bien.");

                // Analizar tono y formato si hay copy asociado
                if (publishingJob.GeneratedCopyId.HasValue)
                {
                    var copy = await _copyRepository.GetByIdAsync(
                        publishingJob.GeneratedCopyId.Value, tenantId, cancellationToken);
                    
                    if (copy != null)
                    {
                        var toneLearning = $"Tono '{copy.CopyType}' funcionó bien en {publishingJob.Channel}. " +
                                         $"Engagement: {summary.AverageEngagementRate:F2}%.";
                        
                        learnings.Add(toneLearning);

                        // Guardar preferencia de tono
                        var tonePreference = $"Preferencia de tono detectada: {copy.CopyType} en {publishingJob.Channel} " +
                                           $"genera buen engagement ({summary.AverageEngagementRate:F2}%).";

                        await _memoryService.SaveLearningMemoryAsync(
                            tenantId,
                            tonePreference,
                            new List<string> { "Preference", "Tone", copy.CopyType, publishingJob.Channel },
                            7,
                            cancellationToken);
                    }
                }
            }
            else if (!isSuccessful)
            {
                learnings.Add($"Post en {publishingJob.Channel} tuvo bajo rendimiento " +
                            $"({summary.AverageEngagementRate?.ToString("F2") ?? "N/A"}% engagement). " +
                            $"Revisar contenido, timing o audiencia.");
            }

            // Aprendizaje sobre hashtags
            if (!string.IsNullOrEmpty(publishingJob.Hashtags) && isSuccessful)
            {
                learnings.Add($"Hashtags usados en post exitoso: {publishingJob.Hashtags}");
            }

            // Guardar aprendizajes
            foreach (var learning in learnings)
            {
                var tags = new List<string> { "Metrics", "Post", publishingJob.Channel };
                if (isSuccessful) tags.Add("Success");
                else tags.Add("NeedsImprovement");

                var context = new Dictionary<string, object>
                {
                    { "PublishingJobId", publishingJobId },
                    { "Channel", publishingJob.Channel },
                    { "TotalEngagement", summary.TotalEngagement },
                    { "AverageEngagementRate", summary.AverageEngagementRate },
                    { "IsSuccessful", isSuccessful }
                };

                await _memoryService.SaveLearningMemoryAsync(
                    tenantId,
                    learning,
                    tags,
                    isSuccessful ? 7 : 5,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprender de métricas de publicación {PublishingJobId}", publishingJobId);
            throw;
        }
    }

    public async Task ProcessLearningFromRecentMetricsAsync(
        Guid tenantId,
        int daysToAnalyze = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de aprendizaje desde métricas recientes (últimos {Days} días)", daysToAnalyze);

            var fromDate = DateTime.UtcNow.AddDays(-daysToAnalyze).Date;
            var toDate = DateTime.UtcNow.Date;

            // Obtener todas las campañas con métricas recientes
            var campaignsSummaries = await _metricsService.GetAllCampaignsMetricsAsync(
                tenantId, fromDate, toDate, cancellationToken);

            // Procesar cada campaña
            foreach (var summary in campaignsSummaries)
            {
                if (summary.DaysWithMetrics > 0)
                {
                    await LearnFromCampaignMetricsAsync(tenantId, summary.CampaignId, cancellationToken);
                }
            }

            // Obtener publicaciones con métricas recientes
            var campaigns = await _campaignRepository.FindAsync(
                c => c.TenantId == tenantId,
                tenantId,
                cancellationToken);

            foreach (var campaign in campaigns)
            {
                var publishingJobs = await _publishingJobRepository.FindAsync(
                    p => p.CampaignId == campaign.Id && 
                         p.TenantId == tenantId &&
                         p.Status == "Success" &&
                         p.PublishedDate.HasValue &&
                         p.PublishedDate.Value.Date >= fromDate,
                    tenantId,
                    cancellationToken);

                foreach (var job in publishingJobs)
                {
                    var jobMetrics = await _metricsService.GetPublishingJobMetricsAsync(
                        tenantId, job.Id, fromDate, toDate, cancellationToken);

                    if (jobMetrics.Any())
                    {
                        await LearnFromPublishingJobMetricsAsync(tenantId, job.Id, cancellationToken);
                    }
                }
            }

            _logger.LogInformation("Proceso de aprendizaje completado para tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en proceso de aprendizaje desde métricas recientes");
            throw;
        }
    }
}

