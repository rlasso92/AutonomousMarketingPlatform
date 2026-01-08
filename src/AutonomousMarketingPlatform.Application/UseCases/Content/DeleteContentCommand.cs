using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using ContentEntity = AutonomousMarketingPlatform.Domain.Entities.Content;

namespace AutonomousMarketingPlatform.Application.UseCases.Content;

/// <summary>
/// Excepción para recursos no encontrados.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Comando para eliminar contenido.
/// </summary>
public class DeleteContentCommand : IRequest<bool>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
}

/// <summary>
/// Handler para eliminar contenido.
/// </summary>
public class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, bool>
{
    private readonly IRepository<ContentEntity> _contentRepository;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteContentCommandHandler> _logger;

    public DeleteContentCommandHandler(
        IRepository<ContentEntity> contentRepository,
        ISecurityService securityService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<DeleteContentCommandHandler> logger)
    {
        _contentRepository = contentRepository;
        _securityService = securityService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        // Obtener contenido
        var content = await _contentRepository.GetByIdAsync(request.ContentId, request.TenantId, cancellationToken);
        if (content == null)
        {
            throw new NotFoundException($"Contenido {request.ContentId} no encontrado");
        }

        // Soft delete (marcar como inactivo)
        content.IsActive = false;
        content.UpdatedAt = DateTime.UtcNow;

        await _contentRepository.UpdateAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contenido eliminado (soft delete): {ContentId} por usuario {UserId} en tenant {TenantId}",
            content.Id, request.UserId, request.TenantId);

        // Auditoría
        await _auditService.LogAsync(
            request.TenantId,
            "DeleteContent",
            "Content",
            content.Id,
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

