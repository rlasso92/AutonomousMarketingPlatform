using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Campaigns;

/// <summary>
/// Comando para eliminar una campaña.
/// </summary>
public class DeleteCampaignCommand : IRequest<bool>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid CampaignId { get; set; }
}

/// <summary>
/// Handler para eliminar campaña.
/// </summary>
public class DeleteCampaignCommandHandler : IRequestHandler<DeleteCampaignCommand, bool>
{
    private readonly IRepository<Domain.Entities.Campaign> _campaignRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCampaignCommandHandler> _logger;

    public DeleteCampaignCommandHandler(
        IRepository<Domain.Entities.Campaign> campaignRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCampaignCommandHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Obtener campaña
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, request.TenantId, cancellationToken);
        if (campaign == null)
        {
            throw new NotFoundException($"Campaña {request.CampaignId} no encontrada");
        }

        // Validar que no esté activa (solo se pueden eliminar Draft o Archived)
        if (campaign.Status == "Active" || campaign.Status == "Paused")
        {
            throw new InvalidOperationException("No se puede eliminar una campaña activa o pausada. Debe archivarla primero.");
        }

        // Soft delete (marcar como inactiva)
        campaign.IsActive = false;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _campaignRepository.UpdateAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaña eliminada (soft delete): {CampaignId} por usuario {UserId} en tenant {TenantId}",
            campaign.Id, request.UserId, request.TenantId);

        // Auditoría
        await _auditService.LogAsync(
            request.TenantId,
            "DeleteCampaign",
            "Campaign",
            campaign.Id,
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

