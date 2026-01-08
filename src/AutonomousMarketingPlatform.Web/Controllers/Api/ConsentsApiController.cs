using AutonomousMarketingPlatform.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers.Api;

/// <summary>
/// Controlador API para gestión de consentimientos.
/// Usado por workflows n8n y otras integraciones externas.
/// </summary>
/// <remarks>
/// Este endpoint puede ser llamado desde n8n sin autenticación de usuario,
/// ya que n8n actúa como servicio interno del sistema.
/// 
/// NOTA DE SEGURIDAD: En producción, este endpoint debería tener autenticación por API key
/// o estar protegido por una red privada. Por ahora se permite acceso sin autenticación
/// para facilitar la integración con n8n en desarrollo.
/// </remarks>
[ApiController]
[Route("api/Consents")]
[AllowAnonymous]
public class ConsentsApiController : ControllerBase
{
    private readonly IConsentValidationService _consentValidationService;
    private readonly ILogger<ConsentsApiController> _logger;

    public ConsentsApiController(
        IConsentValidationService consentValidationService,
        ILogger<ConsentsApiController> logger)
    {
        _consentValidationService = consentValidationService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica los consentimientos requeridos para marketing autónomo.
    /// Endpoint usado por workflows n8n para validar consentimientos antes de continuar.
    /// </summary>
    /// <param name="tenantId">ID del tenant</param>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado de los consentimientos de IA y publicación</returns>
    /// <response code="200">Retorna el estado de los consentimientos</response>
    /// <response code="400">Si tenantId o userId no son válidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("check")]
    [ProducesResponseType(typeof(ConsentCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckConsents(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar que los parámetros sean válidos
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("CheckConsents llamado con tenantId vacío");
                return BadRequest(new { error = "tenantId is required and must be a valid GUID" });
            }

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("CheckConsents llamado con userId vacío");
                return BadRequest(new { error = "userId is required and must be a valid GUID" });
            }

            // Validar consentimiento para uso de IA
            var aiConsent = await _consentValidationService.ValidateConsentAsync(
                userId,
                tenantId,
                "AIGeneration",
                cancellationToken);

            // Validar consentimiento para publicación automática
            var publishingConsent = await _consentValidationService.ValidateConsentAsync(
                userId,
                tenantId,
                "AutoPublishing",
                cancellationToken);

            if (!aiConsent || !publishingConsent)
            {
                return Ok(new
                {
                    success = false,
                    reason = "Missing required consents",
                    aiConsent,
                    publishingConsent
                });
            }

            _logger.LogInformation(
                "Consent check for User {UserId} in Tenant {TenantId}: AI={AiConsent}, Publishing={PublishingConsent}",
                userId,
                tenantId,
                aiConsent,
                publishingConsent);

            var response = new ConsentCheckResponse
            {
                AiConsent = aiConsent,
                PublishingConsent = publishingConsent
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al verificar consentimientos para User {UserId} en Tenant {TenantId}",
                userId,
                tenantId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Internal server error",
                    message = "Failed to check consents from backend"
                });
        }
    }
}

/// <summary>
/// Respuesta del endpoint de verificación de consentimientos.
/// </summary>
public class ConsentCheckResponse
{
    /// <summary>
    /// Indica si el usuario tiene consentimiento para uso de IA (AIGeneration).
    /// </summary>
    public bool AiConsent { get; set; }

    /// <summary>
    /// Indica si el usuario tiene consentimiento para publicación automática (AutoPublishing).
    /// </summary>
    public bool PublishingConsent { get; set; }
}

