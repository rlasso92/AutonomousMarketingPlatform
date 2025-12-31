using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Consents;

/// <summary>
/// Comando para otorgar un consentimiento.
/// </summary>
public class GrantConsentCommand : IRequest<ConsentDto>
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string? ConsentVersion { get; set; }
    public string? IpAddress { get; set; }
    /// <summary>
    /// ID del usuario que otorga el consentimiento (para auditoría).
    /// Si es null, se asume que el usuario otorga su propio consentimiento.
    /// </summary>
    public Guid? GrantedByUserId { get; set; }
}

/// <summary>
/// Handler para otorgar consentimiento.
/// </summary>
public class GrantConsentCommandHandler : IRequestHandler<GrantConsentCommand, ConsentDto>
{
    private readonly IRepository<Consent> _consentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GrantConsentCommandHandler> _logger;

    public GrantConsentCommandHandler(
        IRepository<Consent> consentRepository,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        ILogger<GrantConsentCommandHandler> logger)
    {
        _consentRepository = consentRepository;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConsentDto> Handle(GrantConsentCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario existe
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new UnauthorizedAccessException($"Usuario con ID {request.UserId} no encontrado.");
        }
        
        // Si el consentimiento es otorgado por otro usuario (GrantedByUserId != null y != UserId),
        // verificar que el usuario que otorga es super admin
        if (request.GrantedByUserId.HasValue && request.GrantedByUserId.Value != request.UserId)
        {
            var grantedByUser = await _userManager.FindByIdAsync(request.GrantedByUserId.Value.ToString());
            if (grantedByUser == null)
            {
                throw new UnauthorizedAccessException($"Usuario que otorga el consentimiento (ID: {request.GrantedByUserId.Value}) no encontrado.");
            }
            
            // Verificar que el usuario que otorga es super admin (TenantId == Guid.Empty)
            if (grantedByUser.TenantId != Guid.Empty)
            {
                throw new UnauthorizedAccessException("Solo los super administradores pueden otorgar consentimientos a otros usuarios.");
            }
        }
        
        // Si el TenantId no coincide, usar el TenantId del usuario en la base de datos
        // Esto puede pasar si hay un problema con los claims, pero el usuario existe
        var effectiveTenantId = user.TenantId != Guid.Empty ? user.TenantId : request.TenantId;

        // Obtener IP si no se proporcionó
        var ipAddress = request.IpAddress ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // Buscar consentimiento existente
        var existingConsents = await _consentRepository.FindAsync(
            c => c.UserId == request.UserId && c.ConsentType == request.ConsentType,
            effectiveTenantId,
            cancellationToken);

        var existingConsent = existingConsents.FirstOrDefault();

        Consent consent;
        if (existingConsent != null)
        {
            // Actualizar consentimiento existente
            existingConsent.IsGranted = true;
            existingConsent.GrantedAt = DateTime.UtcNow;
            existingConsent.RevokedAt = null;
            existingConsent.ConsentVersion = request.ConsentVersion ?? existingConsent.ConsentVersion;
            existingConsent.IpAddress = ipAddress;
            await _consentRepository.UpdateAsync(existingConsent, cancellationToken);
            consent = existingConsent;
            _logger.LogInformation("Consentimiento existente actualizado: ConsentId={ConsentId}, UserId={UserId}, ConsentType={ConsentType}", 
                consent.Id, request.UserId, request.ConsentType);
        }
        else
        {
            // Crear nuevo consentimiento
            consent = new Consent
            {
                TenantId = effectiveTenantId,
                UserId = request.UserId,
                ConsentType = request.ConsentType,
                IsGranted = true,
                GrantedAt = DateTime.UtcNow,
                ConsentVersion = request.ConsentVersion ?? "1.0",
                IpAddress = ipAddress
            };
            await _consentRepository.AddAsync(consent, cancellationToken);
            _logger.LogInformation("Nuevo consentimiento creado: UserId={UserId}, ConsentType={ConsentType}, TenantId={TenantId}", 
                request.UserId, request.ConsentType, effectiveTenantId);
        }

        // Guardar cambios en la base de datos
        _logger.LogInformation("Guardando cambios en la base de datos...");
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
            ConsentVersion = consent.ConsentVersion
        };
    }
}

