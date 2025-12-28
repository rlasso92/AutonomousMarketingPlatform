using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de métricas.
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly IRepository<CampaignMetrics> _campaignMetricsRepository;
    private readonly IRepository<PublishingJobMetrics> _publishingJobMetricsRepository;
    private readonly IRepository<Campaign> _campaignRepository;
    private readonly IRepository<PublishingJob> _publishingJobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        IRepository<CampaignMetrics> campaignMetricsRepository,
        IRepository<PublishingJobMetrics> publishingJobMetricsRepository,
        IRepository<Campaign> campaignRepository,
        IRepository<PublishingJob> publishingJobRepository,
        IUnitOfWork unitOfWork,
        ILogger<MetricsService> logger)
    {
        _campaignMetricsRepository = campaignMetricsRepository;
        _publishingJobMetricsRepository = publishingJobMetricsRepository;
        _campaignRepository = campaignRepository;
        _publishingJobRepository = publishingJobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CampaignMetricsDto> RegisterCampaignMetricsAsync(
        Guid tenantId,
        RegisterCampaignMetricsDto metrics,
        CancellationToken cancellationToken = default)
    {
        // Verificar que la campaña existe y pertenece al tenant
        var campaign = await _campaignRepository.GetByIdAsync(metrics.CampaignId, tenantId, cancellationToken);
        if (campaign == null)
        {
            throw new NotFoundException($"Campaña {metrics.CampaignId} no encontrada");
        }

        // Normalizar fecha (solo fecha, sin hora)
        var metricDate = metrics.MetricDate.Date;

        // Buscar métricas existentes para esta fecha
        var existingMetrics = await _campaignMetricsRepository.FindAsync(
            m => m.CampaignId == metrics.CampaignId && 
                 m.MetricDate.Date == metricDate &&
                 m.TenantId == tenantId,
            tenantId,
            cancellationToken);

        CampaignMetrics campaignMetrics;
        var existing = existingMetrics.FirstOrDefault();

        if (existing != null)
        {
            // Actualizar métricas existentes
            existing.Impressions = metrics.Impressions;
            existing.Clicks = metrics.Clicks;
            existing.Likes = metrics.Likes;
            existing.Comments = metrics.Comments;
            existing.Shares = metrics.Shares;
            existing.Engagement = metrics.Likes + metrics.Comments + metrics.Shares;
            existing.Source = metrics.Source ?? "Manual";
            existing.Notes = metrics.Notes;
            existing.IsManualEntry = true;
            campaignMetrics = existing;
        }
        else
        {
            // Crear nuevas métricas
            campaignMetrics = new CampaignMetrics
            {
                TenantId = tenantId,
                CampaignId = metrics.CampaignId,
                MetricDate = metricDate,
                Impressions = metrics.Impressions,
                Clicks = metrics.Clicks,
                Likes = metrics.Likes,
                Comments = metrics.Comments,
                Shares = metrics.Shares,
                Engagement = metrics.Likes + metrics.Comments + metrics.Shares,
                Source = metrics.Source ?? "Manual",
                Notes = metrics.Notes,
                IsManualEntry = true,
                ActivePosts = 0 // Se calculará después
            };

            await _campaignMetricsRepository.AddAsync(campaignMetrics, cancellationToken);
        }

        // Calcular número de posts activos en esta fecha
        var activePosts = await _publishingJobRepository.FindAsync(
            p => p.CampaignId == metrics.CampaignId &&
                 p.Status == "Success" &&
                 p.PublishedDate.HasValue &&
                 p.PublishedDate.Value.Date <= metricDate &&
                 p.TenantId == tenantId,
            tenantId,
            cancellationToken);
        campaignMetrics.ActivePosts = activePosts.Count();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Métricas de campaña registradas: CampaignId={CampaignId}, Date={Date}, Impressions={Impressions}",
            metrics.CampaignId, metricDate, metrics.Impressions);

        return MapToCampaignMetricsDto(campaignMetrics, campaign);
    }

    public async Task<PublishingJobMetricsDto> RegisterPublishingJobMetricsAsync(
        Guid tenantId,
        RegisterPublishingJobMetricsDto metrics,
        CancellationToken cancellationToken = default)
    {
        // Verificar que la publicación existe y pertenece al tenant
        var publishingJob = await _publishingJobRepository.GetByIdAsync(
            metrics.PublishingJobId, tenantId, cancellationToken);
        if (publishingJob == null)
        {
            throw new NotFoundException($"Publicación {metrics.PublishingJobId} no encontrada");
        }

        // Normalizar fecha
        var metricDate = metrics.MetricDate.Date;

        // Buscar métricas existentes
        var existingMetrics = await _publishingJobMetricsRepository.FindAsync(
            m => m.PublishingJobId == metrics.PublishingJobId &&
                 m.MetricDate.Date == metricDate &&
                 m.TenantId == tenantId,
            tenantId,
            cancellationToken);

        PublishingJobMetrics jobMetrics;
        var existing = existingMetrics.FirstOrDefault();

        if (existing != null)
        {
            // Actualizar
            existing.Impressions = metrics.Impressions;
            existing.Clicks = metrics.Clicks;
            existing.Likes = metrics.Likes;
            existing.Comments = metrics.Comments;
            existing.Shares = metrics.Shares;
            existing.Engagement = metrics.Likes + metrics.Comments + metrics.Shares;
            existing.ClickThroughRate = metrics.Impressions > 0 
                ? (decimal)metrics.Clicks / metrics.Impressions * 100 
                : null;
            existing.EngagementRate = metrics.Impressions > 0 
                ? (decimal)existing.Engagement / metrics.Impressions * 100 
                : null;
            existing.Source = metrics.Source ?? "Manual";
            existing.Notes = metrics.Notes;
            existing.IsManualEntry = true;
            jobMetrics = existing;
        }
        else
        {
            // Crear nuevas
            var engagement = metrics.Likes + metrics.Comments + metrics.Shares;
            jobMetrics = new PublishingJobMetrics
            {
                TenantId = tenantId,
                PublishingJobId = metrics.PublishingJobId,
                MetricDate = metricDate,
                Impressions = metrics.Impressions,
                Clicks = metrics.Clicks,
                Likes = metrics.Likes,
                Comments = metrics.Comments,
                Shares = metrics.Shares,
                Engagement = engagement,
                ClickThroughRate = metrics.Impressions > 0 
                    ? (decimal)metrics.Clicks / metrics.Impressions * 100 
                    : null,
                EngagementRate = metrics.Impressions > 0 
                    ? (decimal)engagement / metrics.Impressions * 100 
                    : null,
                Source = metrics.Source ?? "Manual",
                Notes = metrics.Notes,
                IsManualEntry = true
            };

            await _publishingJobMetricsRepository.AddAsync(jobMetrics, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Métricas de publicación registradas: PublishingJobId={PublishingJobId}, Date={Date}, Impressions={Impressions}",
            metrics.PublishingJobId, metricDate, metrics.Impressions);

        return MapToPublishingJobMetricsDto(jobMetrics, publishingJob);
    }

    public async Task<List<CampaignMetricsDto>> GetCampaignMetricsAsync(
        Guid tenantId,
        Guid campaignId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = await _campaignMetricsRepository.FindAsync(
            m => m.CampaignId == campaignId && m.TenantId == tenantId,
            tenantId,
            cancellationToken);

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.MetricDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.MetricDate <= toDate.Value.Date);
        }

        var metrics = query.OrderBy(m => m.MetricDate).ToList();
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, tenantId, cancellationToken);

        return metrics.Select(m => MapToCampaignMetricsDto(m, campaign)).ToList();
    }

    public async Task<List<PublishingJobMetricsDto>> GetPublishingJobMetricsAsync(
        Guid tenantId,
        Guid publishingJobId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = await _publishingJobMetricsRepository.FindAsync(
            m => m.PublishingJobId == publishingJobId && m.TenantId == tenantId,
            tenantId,
            cancellationToken);

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.MetricDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.MetricDate <= toDate.Value.Date);
        }

        var metrics = query.OrderBy(m => m.MetricDate).ToList();
        var publishingJob = await _publishingJobRepository.GetByIdAsync(publishingJobId, tenantId, cancellationToken);

        return metrics.Select(m => MapToPublishingJobMetricsDto(m, publishingJob)).ToList();
    }

    public async Task<CampaignMetricsSummaryDto> GetCampaignMetricsSummaryAsync(
        Guid tenantId,
        Guid campaignId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _campaignRepository.GetByIdAsync(campaignId, tenantId, cancellationToken);
        if (campaign == null)
        {
            throw new NotFoundException($"Campaña {campaignId} no encontrada");
        }

        var dailyMetrics = await GetCampaignMetricsAsync(tenantId, campaignId, fromDate, toDate, cancellationToken);

        var summary = new CampaignMetricsSummaryDto
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            TotalImpressions = dailyMetrics.Sum(m => m.Impressions),
            TotalClicks = dailyMetrics.Sum(m => m.Clicks),
            TotalEngagement = dailyMetrics.Sum(m => m.Engagement),
            TotalLikes = dailyMetrics.Sum(m => m.Likes),
            TotalComments = dailyMetrics.Sum(m => m.Comments),
            TotalShares = dailyMetrics.Sum(m => m.Shares),
            DaysWithMetrics = dailyMetrics.Count,
            DailyMetrics = dailyMetrics
        };

        // Calcular tasas promedio
        if (summary.TotalImpressions > 0)
        {
            summary.AverageClickThroughRate = (decimal)summary.TotalClicks / summary.TotalImpressions * 100;
            summary.AverageEngagementRate = (decimal)summary.TotalEngagement / summary.TotalImpressions * 100;
        }

        // Contar posts totales
        var posts = await _publishingJobRepository.FindAsync(
            p => p.CampaignId == campaignId && p.TenantId == tenantId,
            tenantId,
            cancellationToken);
        summary.TotalPosts = posts.Count(p => p.Status == "Success");

        return summary;
    }

    public async Task<PublishingJobMetricsSummaryDto> GetPublishingJobMetricsSummaryAsync(
        Guid tenantId,
        Guid publishingJobId,
        CancellationToken cancellationToken = default)
    {
        var publishingJob = await _publishingJobRepository.GetByIdAsync(
            publishingJobId, tenantId, cancellationToken);
        if (publishingJob == null)
        {
            throw new NotFoundException($"Publicación {publishingJobId} no encontrada");
        }

        var dailyMetrics = await GetPublishingJobMetricsAsync(tenantId, publishingJobId, null, null, cancellationToken);

        var summary = new PublishingJobMetricsSummaryDto
        {
            PublishingJobId = publishingJobId,
            Content = publishingJob.Content,
            Channel = publishingJob.Channel,
            PublishedDate = publishingJob.PublishedDate,
            TotalImpressions = dailyMetrics.Sum(m => m.Impressions),
            TotalClicks = dailyMetrics.Sum(m => m.Clicks),
            TotalEngagement = dailyMetrics.Sum(m => m.Engagement),
            TotalLikes = dailyMetrics.Sum(m => m.Likes),
            TotalComments = dailyMetrics.Sum(m => m.Comments),
            TotalShares = dailyMetrics.Sum(m => m.Shares),
            DaysWithMetrics = dailyMetrics.Count,
            DailyMetrics = dailyMetrics
        };

        if (summary.TotalImpressions > 0)
        {
            summary.AverageClickThroughRate = (decimal)summary.TotalClicks / summary.TotalImpressions * 100;
            summary.AverageEngagementRate = (decimal)summary.TotalEngagement / summary.TotalImpressions * 100;
        }

        return summary;
    }

    public async Task<List<CampaignMetricsSummaryDto>> GetAllCampaignsMetricsAsync(
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var campaigns = await _campaignRepository.FindAsync(
            c => c.TenantId == tenantId,
            tenantId,
            cancellationToken);

        var summaries = new List<CampaignMetricsSummaryDto>();

        foreach (var campaign in campaigns)
        {
            var summary = await GetCampaignMetricsSummaryAsync(tenantId, campaign.Id, fromDate, toDate, cancellationToken);
            summaries.Add(summary);
        }

        return summaries.OrderByDescending(s => s.TotalImpressions).ToList();
    }

    private CampaignMetricsDto MapToCampaignMetricsDto(CampaignMetrics metrics, Campaign? campaign)
    {
        return new CampaignMetricsDto
        {
            Id = metrics.Id,
            CampaignId = metrics.CampaignId,
            CampaignName = campaign?.Name,
            MetricDate = metrics.MetricDate,
            Impressions = metrics.Impressions,
            Clicks = metrics.Clicks,
            Engagement = metrics.Engagement,
            Likes = metrics.Likes,
            Comments = metrics.Comments,
            Shares = metrics.Shares,
            ActivePosts = metrics.ActivePosts,
            IsManualEntry = metrics.IsManualEntry,
            Source = metrics.Source,
            Notes = metrics.Notes,
            ClickThroughRate = metrics.Impressions > 0 
                ? (decimal)metrics.Clicks / metrics.Impressions * 100 
                : null,
            EngagementRate = metrics.Impressions > 0 
                ? (decimal)metrics.Engagement / metrics.Impressions * 100 
                : null,
            CreatedAt = metrics.CreatedAt
        };
    }

    private PublishingJobMetricsDto MapToPublishingJobMetricsDto(PublishingJobMetrics metrics, PublishingJob? publishingJob)
    {
        return new PublishingJobMetricsDto
        {
            Id = metrics.Id,
            PublishingJobId = metrics.PublishingJobId,
            PublishingJobContent = publishingJob?.Content,
            Channel = publishingJob?.Channel,
            MetricDate = metrics.MetricDate,
            Impressions = metrics.Impressions,
            Clicks = metrics.Clicks,
            Engagement = metrics.Engagement,
            Likes = metrics.Likes,
            Comments = metrics.Comments,
            Shares = metrics.Shares,
            ClickThroughRate = metrics.ClickThroughRate,
            EngagementRate = metrics.EngagementRate,
            IsManualEntry = metrics.IsManualEntry,
            Source = metrics.Source,
            Notes = metrics.Notes,
            CreatedAt = metrics.CreatedAt
        };
    }
}

/// <summary>
/// Excepción para recursos no encontrados.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

