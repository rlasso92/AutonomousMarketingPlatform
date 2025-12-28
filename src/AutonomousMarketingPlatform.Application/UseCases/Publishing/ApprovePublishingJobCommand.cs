using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Publishing;

/// <summary>
/// Comando para aprobar un trabajo de publicación (marcar como publicado manualmente).
/// </summary>
public class ApprovePublishingJobCommand : IRequest<bool>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid JobId { get; set; }
    public string? PublishedUrl { get; set; }
    public string? ExternalPostId { get; set; }
}

/// <summary>
/// Handler para aprobar trabajo de publicación.
/// </summary>
public class ApprovePublishingJobCommandHandler : IRequestHandler<ApprovePublishingJobCommand, bool>
{
    private readonly IRepository<Domain.Entities.PublishingJob> _publishingJobRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovePublishingJobCommandHandler> _logger;

    public ApprovePublishingJobCommandHandler(
        IRepository<Domain.Entities.PublishingJob> publishingJobRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<ApprovePublishingJobCommandHandler> logger)
    {
        _publishingJobRepository = publishingJobRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApprovePublishingJobCommand request, CancellationToken cancellationToken)
    {
        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Obtener job
        var job = await _publishingJobRepository.GetByIdAsync(request.JobId, request.TenantId, cancellationToken);
        if (job == null)
        {
            throw new NotFoundException($"PublishingJob {request.JobId} no encontrado");
        }

        if (job.Status != "RequiresApproval")
        {
            throw new InvalidOperationException("El job no requiere aprobación o ya fue procesado");
        }

        // Marcar como aprobado y publicado
        job.Status = "Success";
        job.PublishedDate = DateTime.UtcNow;
        job.PublishedUrl = request.PublishedUrl;
        job.ExternalPostId = request.ExternalPostId;
        job.ApprovedAt = DateTime.UtcNow;
        job.ApprovedBy = request.UserId;

        await _publishingJobRepository.UpdateAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("PublishingJob {JobId} aprobado por usuario {UserId}", request.JobId, request.UserId);

        // Auditoría
        await _auditService.LogAsync(
            request.TenantId,
            "ApprovePublishingJob",
            "PublishingJob",
            job.Id,
            request.UserId,
            null,
            null,
            null,
            null,
            "Success",
            null,
            null,
            null,
            cancellationToken);

        return true;
    }
}

