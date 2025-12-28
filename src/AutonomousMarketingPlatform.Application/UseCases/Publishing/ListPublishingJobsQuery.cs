using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Publishing;

/// <summary>
/// Query para listar trabajos de publicación.
/// </summary>
public class ListPublishingJobsQuery : IRequest<List<PublishingJobListDto>>
{
    public Guid TenantId { get; set; }
    public Guid? CampaignId { get; set; }
    public string? Status { get; set; }
    public string? Channel { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

/// <summary>
/// Handler para listar trabajos de publicación.
/// </summary>
public class ListPublishingJobsQueryHandler : IRequestHandler<ListPublishingJobsQuery, List<PublishingJobListDto>>
{
    private readonly IRepository<Domain.Entities.PublishingJob> _publishingJobRepository;
    private readonly ILogger<ListPublishingJobsQueryHandler> _logger;

    public ListPublishingJobsQueryHandler(
        IRepository<Domain.Entities.PublishingJob> publishingJobRepository,
        ILogger<ListPublishingJobsQueryHandler> logger)
    {
        _publishingJobRepository = publishingJobRepository;
        _logger = logger;
    }

    public async Task<List<PublishingJobListDto>> Handle(ListPublishingJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _publishingJobRepository.FindAsync(
            j => j.TenantId == request.TenantId &&
                 j.IsActive &&
                 (request.CampaignId == null || j.CampaignId == request.CampaignId) &&
                 (string.IsNullOrEmpty(request.Status) || j.Status == request.Status) &&
                 (string.IsNullOrEmpty(request.Channel) || j.Channel == request.Channel),
            request.TenantId,
            cancellationToken);

        return jobs
            .OrderByDescending(j => j.ScheduledDate ?? j.CreatedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(j => new PublishingJobListDto
            {
                Id = j.Id,
                Channel = j.Channel,
                Status = j.Status,
                ScheduledDate = j.ScheduledDate,
                PublishedDate = j.PublishedDate,
                PublishedUrl = j.PublishedUrl,
                ErrorMessage = j.ErrorMessage,
                RequiresApproval = j.RequiresApproval,
                CreatedAt = j.CreatedAt
            })
            .ToList();
    }
}

