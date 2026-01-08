using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MarketingPackEntity = AutonomousMarketingPlatform.Domain.Entities.MarketingPack;

namespace AutonomousMarketingPlatform.Application.UseCases.AI;

/// <summary>
/// Comando para eliminar un MarketingPack.
/// </summary>
public class DeleteMarketingPackCommand : IRequest<bool>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid MarketingPackId { get; set; }
}

/// <summary>
/// Handler para eliminar MarketingPack.
/// </summary>
public class DeleteMarketingPackCommandHandler : IRequestHandler<DeleteMarketingPackCommand, bool>
{
    private readonly IRepository<MarketingPackEntity> _marketingPackRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteMarketingPackCommandHandler> _logger;

    public DeleteMarketingPackCommandHandler(
        IRepository<MarketingPackEntity> marketingPackRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteMarketingPackCommandHandler> logger)
    {
        _marketingPackRepository = marketingPackRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteMarketingPackCommand request, CancellationToken cancellationToken)
    {
        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Obtener MarketingPack
        var marketingPack = await _marketingPackRepository.GetByIdAsync(
            request.MarketingPackId, 
            request.TenantId, 
            cancellationToken);
        
        if (marketingPack == null)
        {
            throw new NotFoundException($"MarketingPack {request.MarketingPackId} no encontrado");
        }

        // Soft delete (marcar como inactivo)
        marketingPack.IsActive = false;
        marketingPack.UpdatedAt = DateTime.UtcNow;

        await _marketingPackRepository.UpdateAsync(marketingPack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MarketingPack eliminado (soft delete): {MarketingPackId} por usuario {UserId} en tenant {TenantId}",
            marketingPack.Id, request.UserId, request.TenantId);

        // Auditoría
        await _auditService.LogAsync(
            request.TenantId,
            "DeleteMarketingPack",
            "MarketingPack",
            marketingPack.Id,
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

