using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Publishing;

/// <summary>
/// Query para obtener un trabajo de publicación por ID.
/// </summary>
public class GetPublishingJobQuery : IRequest<PublishingJobDto?>
{
    public Guid TenantId { get; set; }
    public Guid JobId { get; set; }
}

/// <summary>
/// Handler para obtener trabajo de publicación.
/// </summary>
public class GetPublishingJobQueryHandler : IRequestHandler<GetPublishingJobQuery, PublishingJobDto?>
{
    private readonly IRepository<Domain.Entities.PublishingJob> _publishingJobRepository;
    private readonly ILogger<GetPublishingJobQueryHandler> _logger;

    public GetPublishingJobQueryHandler(
        IRepository<Domain.Entities.PublishingJob> publishingJobRepository,
        ILogger<GetPublishingJobQueryHandler> logger)
    {
        _publishingJobRepository = publishingJobRepository;
        _logger = logger;
    }

    public async Task<PublishingJobDto?> Handle(GetPublishingJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _publishingJobRepository.GetByIdAsync(request.JobId, request.TenantId, cancellationToken);
        
        if (job == null || !job.IsActive)
        {
            return null;
        }

        return new PublishingJobDto
        {
            Id = job.Id,
            CampaignId = job.CampaignId,
            MarketingPackId = job.MarketingPackId,
            GeneratedCopyId = job.GeneratedCopyId,
            Channel = job.Channel,
            Status = job.Status,
            ScheduledDate = job.ScheduledDate,
            PublishedDate = job.PublishedDate,
            PublishedUrl = job.PublishedUrl,
            Content = job.Content,
            Hashtags = job.Hashtags,
            MediaUrl = job.MediaUrl,
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            RequiresApproval = job.RequiresApproval,
            DownloadUrl = job.DownloadUrl,
            CreatedAt = job.CreatedAt
        };
    }
}

