using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutonomousMarketingPlatform.Application.UseCases.Consents;

/// <summary>
/// Query para obtener todos los consentimientos de un usuario.
/// </summary>
public class GetUserConsentsQuery : IRequest<UserConsentsDto>
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
}

/// <summary>
/// Handler para obtener consentimientos del usuario.
/// </summary>
public class GetUserConsentsQueryHandler : IRequestHandler<GetUserConsentsQuery, UserConsentsDto>
{
    private readonly IRepository<Consent> _consentRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserConsentsQueryHandler(
        IRepository<Consent> consentRepository,
        UserManager<ApplicationUser> userManager)
    {
        _consentRepository = consentRepository;
        _userManager = userManager;
    }

    public async Task<UserConsentsDto> Handle(GetUserConsentsQuery request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario existe
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            // Si el usuario no existe, retornar lista vacía en lugar de lanzar excepción
            // Esto permite que la página de consentimientos se muestre aunque el usuario no tenga consentimientos
            return new UserConsentsDto
            {
                UserId = request.UserId,
                Consents = new List<ConsentDto>
                {
                    new ConsentDto { ConsentType = "AIGeneration", ConsentTypeDisplayName = "Generación de Contenido con IA", Description = "Permite al sistema generar contenido publicitario usando inteligencia artificial.", Required = true, IsGranted = false },
                    new ConsentDto { ConsentType = "DataProcessing", ConsentTypeDisplayName = "Procesamiento de Datos", Description = "Permite procesar y analizar datos para mejorar el marketing.", Required = true, IsGranted = false },
                    new ConsentDto { ConsentType = "AutoPublishing", ConsentTypeDisplayName = "Publicación Automática", Description = "Permite publicar contenido automáticamente en redes sociales.", Required = false, IsGranted = false },
                    new ConsentDto { ConsentType = "Analytics", ConsentTypeDisplayName = "Análisis y Métricas", Description = "Permite recopilar y analizar métricas de rendimiento.", Required = false, IsGranted = false }
                },
                AllRequiredConsentsGranted = false,
                MissingRequiredConsents = new List<string> { "Generación de Contenido con IA", "Procesamiento de Datos" }
            };
        }
        
        // Verificar que el usuario pertenece al tenant
        if (user.TenantId != request.TenantId)
        {
            throw new UnauthorizedAccessException($"El usuario {request.UserId} no pertenece al tenant {request.TenantId}. Tenant del usuario: {user.TenantId}.");
        }

        // Obtener todos los consentimientos del usuario
        var consents = await _consentRepository.FindAsync(
            c => c.UserId == request.UserId,
            request.TenantId,
            cancellationToken);

        // Definir tipos de consentimiento requeridos
        var requiredConsentTypes = new[] { "AIGeneration", "DataProcessing" };
        var allConsentTypes = new[]
        {
            new { Type = "AIGeneration", DisplayName = "Generación de Contenido con IA", Description = "Permite al sistema generar contenido publicitario usando inteligencia artificial.", Required = true },
            new { Type = "DataProcessing", DisplayName = "Procesamiento de Datos", Description = "Permite procesar y analizar datos para mejorar el marketing.", Required = true },
            new { Type = "AutoPublishing", DisplayName = "Publicación Automática", Description = "Permite publicar contenido automáticamente en redes sociales.", Required = false },
            new { Type = "Analytics", DisplayName = "Análisis y Métricas", Description = "Permite recopilar y analizar métricas de rendimiento.", Required = false }
        };

        var consentDtos = allConsentTypes.Select(ct =>
        {
            var existingConsent = consents.FirstOrDefault(c => c.ConsentType == ct.Type);
            return new ConsentDto
            {
                Id = existingConsent?.Id ?? Guid.Empty,
                UserId = request.UserId,
                ConsentType = ct.Type,
                ConsentTypeDisplayName = ct.DisplayName,
                Description = ct.Description,
                IsGranted = existingConsent?.IsGranted ?? false,
                GrantedAt = existingConsent?.GrantedAt,
                RevokedAt = existingConsent?.RevokedAt,
                ConsentVersion = existingConsent?.ConsentVersion,
                IsRequired = ct.Required,
                CanRevoke = existingConsent != null && existingConsent.IsGranted && !ct.Required
            };
        }).ToList();

        var missingRequired = consentDtos
            .Where(c => c.IsRequired && !c.IsGranted)
            .Select(c => c.ConsentTypeDisplayName)
            .ToList();

        return new UserConsentsDto
        {
            UserId = request.UserId,
            Consents = consentDtos,
            AllRequiredConsentsGranted = !missingRequired.Any(),
            MissingRequiredConsents = missingRequired
        };
    }
}

