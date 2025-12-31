using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Consents;
using AutonomousMarketingPlatform.Application.UseCases.Users;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de consentimientos.
/// </summary>
[Authorize]
public class ConsentsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsentsController> _logger;

    public ConsentsController(IMediator mediator, ILogger<ConsentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los consentimientos del usuario actual.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");

        if (!userId.HasValue)
        {
            _logger.LogWarning("Usuario autenticado sin UserId");
            return RedirectToAction("Login", "Account");
        }

        // Super admin puede no tener TenantId
        if (!tenantId.HasValue && !isSuperAdmin)
        {
            _logger.LogWarning("Usuario autenticado sin TenantId y no es super admin");
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var query = new GetUserConsentsQuery
            {
                UserId = userId.Value,
                TenantId = tenantId ?? Guid.Empty
            };

            var result = await _mediator.Send(query);
            
            // Si es super admin, cargar lista de usuarios para poder otorgar consentimientos
            if (isSuperAdmin)
            {
                var usersQuery = new ListUsersQuery { TenantId = Guid.Empty };
                var users = await _mediator.Send(usersQuery);
                ViewBag.Users = users;
                ViewBag.IsSuperAdmin = true;
            }
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consentimientos del usuario {UserId}", userId.Value);
            TempData["ErrorMessage"] = "Error al cargar los consentimientos. Por favor, intente nuevamente.";
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Obtiene los consentimientos de un usuario específico (solo para super admin).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> UserConsents(Guid userId)
    {
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        
        if (!isSuperAdmin)
        {
            _logger.LogWarning("Usuario intentó acceder a consentimientos de otro usuario sin ser super admin");
            TempData["ErrorMessage"] = "No tiene permisos para ver consentimientos de otros usuarios.";
            return RedirectToAction("Index", "Consents");
        }

        try
        {
            // Obtener información del usuario objetivo
            var getUserQuery = new GetUserQuery { UserId = userId };
            var targetUser = await _mediator.Send(getUserQuery);

            var query = new GetUserConsentsQuery
            {
                UserId = userId,
                TenantId = targetUser.TenantId
            };

            var result = await _mediator.Send(query);
            
            // Cargar lista de usuarios para poder cambiar de usuario
            var usersQuery = new ListUsersQuery { TenantId = Guid.Empty };
            var users = await _mediator.Send(usersQuery);
            ViewBag.Users = users;
            ViewBag.IsSuperAdmin = true;
            ViewBag.TargetUser = targetUser;
            
            return View("Index", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consentimientos del usuario {UserId}", userId);
            TempData["ErrorMessage"] = $"Error al cargar los consentimientos: {ex.Message}";
            return RedirectToAction("Index", "Consents");
        }
    }

    /// <summary>
    /// Otorga un consentimiento.
    /// El usuario otorga su propio consentimiento, o un super admin otorga consentimiento a otro usuario.
    /// </summary>
    [HttpPost("Consents/Grant")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grant([FromForm] CreateConsentDto dto)
    {
        _logger.LogInformation("Grant consent llamado. ConsentType={ConsentType}, ConsentVersion={ConsentVersion}, TargetUserId={TargetUserId}", 
            dto?.ConsentType, dto?.ConsentVersion, dto?.TargetUserId);

        var currentUserId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");

        _logger.LogInformation("CurrentUserId={CurrentUserId}, TenantId={TenantId}, IsSuperAdmin={IsSuperAdmin}", 
            currentUserId, tenantId, isSuperAdmin);

        if (!currentUserId.HasValue)
        {
            _logger.LogWarning("Usuario sin UserId. Redirigiendo a Login");
            TempData["ErrorMessage"] = "Error de autenticación. Por favor, inicie sesión nuevamente.";
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrEmpty(dto?.ConsentType))
        {
            _logger.LogWarning("ConsentType vacío o nulo");
            TempData["ErrorMessage"] = "Error: Tipo de consentimiento no especificado.";
            return RedirectToAction("Index", "Consents");
        }

        // Determinar el usuario al que se otorga el consentimiento
        Guid targetUserId;
        Guid effectiveTenantId;
        
        if (dto.TargetUserId.HasValue && dto.TargetUserId.Value != currentUserId.Value)
        {
            // Super admin intentando otorgar consentimiento a otro usuario
            if (!isSuperAdmin)
            {
                _logger.LogWarning("Usuario {UserId} intentó otorgar consentimiento a otro usuario sin ser super admin", currentUserId.Value);
                TempData["ErrorMessage"] = "No tiene permisos para otorgar consentimientos a otros usuarios.";
                return RedirectToAction("Index", "Consents");
            }
            
            targetUserId = dto.TargetUserId.Value;
            
            // Obtener el tenant del usuario objetivo
            try
            {
                var getUserQuery = new GetUserQuery { UserId = targetUserId };
                var targetUser = await _mediator.Send(getUserQuery);
                effectiveTenantId = targetUser.TenantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario objetivo {TargetUserId}", targetUserId);
                TempData["ErrorMessage"] = $"Error: No se pudo encontrar el usuario especificado.";
                return RedirectToAction("Index", "Consents");
            }
        }
        else
        {
            // Usuario otorgando su propio consentimiento
            targetUserId = currentUserId.Value;
            effectiveTenantId = tenantId ?? Guid.Empty;
        }

        try
        {
            var command = new GrantConsentCommand
            {
                UserId = targetUserId,
                TenantId = effectiveTenantId,
                ConsentType = dto.ConsentType,
                ConsentVersion = dto.ConsentVersion ?? "1.0",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                GrantedByUserId = dto.TargetUserId.HasValue && dto.TargetUserId.Value != currentUserId.Value 
                    ? currentUserId.Value 
                    : null
            };

            _logger.LogInformation("Enviando GrantConsentCommand: UserId={UserId}, TenantId={TenantId}, ConsentType={ConsentType}, GrantedByUserId={GrantedByUserId}", 
                command.UserId, command.TenantId, command.ConsentType, command.GrantedByUserId);

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Consentimiento otorgado exitosamente. ConsentId={ConsentId}", result.Id);
            
            if (command.GrantedByUserId.HasValue)
            {
                TempData["SuccessMessage"] = $"Consentimiento otorgado correctamente al usuario.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Consentimiento otorgado correctamente.";
            }
            
            // Si es super admin otorgando a otro usuario, redirigir a la vista de ese usuario
            if (dto.TargetUserId.HasValue && dto.TargetUserId.Value != currentUserId.Value)
            {
                return RedirectToAction("UserConsents", "Consents", new { userId = targetUserId });
            }
            
            return RedirectToAction("Index", "Consents");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al otorgar consentimiento {ConsentType} para usuario {UserId}. Exception={ExceptionType}, Message={Message}", 
                dto?.ConsentType, targetUserId, ex.GetType().Name, ex.Message);
            TempData["ErrorMessage"] = $"Error al otorgar el consentimiento: {ex.Message}";
            
            if (dto.TargetUserId.HasValue && dto.TargetUserId.Value != currentUserId.Value)
            {
                return RedirectToAction("UserConsents", "Consents", new { userId = targetUserId });
            }
            
            return RedirectToAction("Index", "Consents");
        }
    }

    /// <summary>
    /// Revoca un consentimiento.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke([FromForm] string consentType)
    {
        var userId = UserHelper.GetUserId(User);
        var tenantId = UserHelper.GetTenantId(User);

        if (!userId.HasValue || !tenantId.HasValue)
        {
            TempData["ErrorMessage"] = "Error de autenticación. Por favor, inicie sesión nuevamente.";
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var command = new RevokeConsentCommand
            {
                UserId = userId.Value,
                TenantId = tenantId.Value,
                ConsentType = consentType
            };

            var result = await _mediator.Send(command);
            TempData["SuccessMessage"] = $"Consentimiento '{consentType}' revocado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Intento de revocar consentimiento requerido {ConsentType}", consentType);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revocar consentimiento {ConsentType} para usuario {UserId}", consentType, userId.Value);
            TempData["ErrorMessage"] = "Error al revocar el consentimiento. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

