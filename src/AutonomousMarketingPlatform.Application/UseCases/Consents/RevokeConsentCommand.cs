using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Consents;

/// <summary>
/// Comando para revocar un consentimiento.
/// </summary>
public class RevokeConsentCommand : IRequest<ConsentDto>
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
}

/// <summary>
/// Handler para revocar consentimiento.
/// </summary>
public class RevokeConsentCommandHandler : IRequestHandler<RevokeConsentCommand, ConsentDto>
{
    private readonly IRepository<Consent> _consentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeConsentCommandHandler> _logger;

    public RevokeConsentCommandHandler(
        IRepository<Consent> consentRepository,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<RevokeConsentCommandHandler> logger)
    {
        _consentRepository = consentRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConsentDto> Handle(RevokeConsentCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario existe
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null || user.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException("Usuario no encontrado o no pertenece al tenant.");
        }

        // Verificar que el consentimiento no es requerido
        var requiredConsentTypes = new[] { "AIGeneration", "DataProcessing" };
        if (requiredConsentTypes.Contains(request.ConsentType))
        {
            throw new InvalidOperationException($"El consentimiento '{request.ConsentType}' es requerido y no puede ser revocado.");
        }

        // Buscar consentimiento existente
        var existingConsents = await _consentRepository.FindAsync(
            c => c.UserId == request.UserId && c.ConsentType == request.ConsentType,
            request.TenantId,
            cancellationToken);

        var consent = existingConsents.FirstOrDefault();
        if (consent == null || !consent.IsGranted)
        {
            throw new InvalidOperationException("Consentimiento no encontrado o ya revocado.");
        }

        // Revocar consentimiento
        consent.IsGranted = false;
        consent.RevokedAt = DateTime.UtcNow;
        await _consentRepository.UpdateAsync(consent, cancellationToken);

        // Guardar cambios en la base de datos
        _logger.LogInformation("Guardando cambios en la base de datos para revocación de consentimiento...");
        var savedChanges = await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cambios guardados exitosamente. SavedChanges={SavedChanges}, ConsentId={ConsentId}", 
            savedChanges, consent.Id);

        return new ConsentDto
        {
            Id = consent.Id,
            UserId = consent.UserId,
            ConsentType = consent.ConsentType,
            IsGranted = consent.IsGranted,
            GrantedAt = consent.GrantedAt,
            RevokedAt = consent.RevokedAt,
            ConsentVersion = consent.ConsentVersion
        };
    }
}

