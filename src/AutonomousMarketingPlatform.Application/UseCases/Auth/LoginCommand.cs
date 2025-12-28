using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Auth;

/// <summary>
/// Comando para autenticar un usuario.
/// </summary>
public class LoginCommand : IRequest<LoginResultDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public bool RememberMe { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Handler para login.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISecurityService _securityService;
    private readonly IAuditService _auditService;
    private readonly ILogger<LoginCommandHandler> _logger;

    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISecurityService securityService,
        IAuditService auditService,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _securityService = securityService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = new LoginResultDto();

        try
        {
            // Buscar usuario por email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Intento de login con email no encontrado: Email={Email}, TenantId={TenantId}, IP={IP}",
                    request.Email, request.TenantId, request.IpAddress);

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    null,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Email no encontrado",
                    null,
                    null,
                    cancellationToken);

                result.Success = false;
                result.ErrorMessage = "Credenciales inválidas";
                return result;
            }

            // Validar que el usuario pertenece al tenant
            var belongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(user.Id, request.TenantId, cancellationToken);
            if (!belongsToTenant)
            {
                _logger.LogWarning("Intento de login con usuario de otro tenant: UserId={UserId}, TenantId={TenantId}, IP={IP}",
                    user.Id, request.TenantId, request.IpAddress);

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Usuario no pertenece a este tenant",
                    null,
                    null,
                    cancellationToken);

                result.Success = false;
                result.ErrorMessage = "Credenciales inválidas";
                return result;
            }

            // Verificar si el usuario está bloqueado
            if (user.LockoutEndDate.HasValue && user.LockoutEndDate.Value > DateTime.UtcNow)
            {
                result.Success = false;
                result.IsLockedOut = true;
                result.LockoutEnd = user.LockoutEndDate;
                result.ErrorMessage = $"Cuenta bloqueada hasta {user.LockoutEndDate.Value:g}";

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Cuenta bloqueada",
                    null,
                    null,
                    cancellationToken);

                return result;
            }

            // Verificar si el usuario está activo
            if (!user.IsActive)
            {
                result.Success = false;
                result.ErrorMessage = "Cuenta desactivada";

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Cuenta desactivada",
                    null,
                    null,
                    cancellationToken);

                return result;
            }

            // Intentar login
            var signInResult = await _signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                // Login exitoso
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = request.IpAddress;
                user.FailedLoginAttempts = 0; // Resetear intentos fallidos
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Login exitoso: UserId={UserId}, Email={Email}, TenantId={TenantId}, IP={IP}",
                    user.Id, user.Email, request.TenantId, request.IpAddress);

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Success",
                    null,
                    null,
                    null,
                    cancellationToken);

                result.Success = true;
                return result;
            }
            else if (signInResult.IsLockedOut)
            {
                // Usuario bloqueado
                user.LockoutEndDate = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                await _userManager.UpdateAsync(user);

                result.Success = false;
                result.IsLockedOut = true;
                result.LockoutEnd = user.LockoutEndDate;
                result.ErrorMessage = $"Cuenta bloqueada por {LockoutDurationMinutes} minutos debido a múltiples intentos fallidos";

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Cuenta bloqueada por intentos fallidos",
                    null,
                    null,
                    cancellationToken);

                return result;
            }
            else
            {
                // Login fallido
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEndDate = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                }
                await _userManager.UpdateAsync(user);

                result.Success = false;
                result.RemainingAttempts = Math.Max(0, MaxFailedAttempts - user.FailedLoginAttempts);
                result.ErrorMessage = $"Credenciales inválidas. Intentos restantes: {result.RemainingAttempts}";

                _logger.LogWarning("Login fallido: UserId={UserId}, Email={Email}, TenantId={TenantId}, Attempts={Attempts}, IP={IP}",
                    user.Id, user.Email, request.TenantId, user.FailedLoginAttempts, request.IpAddress);

                await _auditService.LogAsync(
                    request.TenantId,
                    "Login",
                    "User",
                    user.Id,
                    null,
                    null,
                    null,
                    request.IpAddress,
                    null,
                    "Failed",
                    "Credenciales inválidas",
                    null,
                    null,
                    cancellationToken);

                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar login: Email={Email}, TenantId={TenantId}", request.Email, request.TenantId);

            result.Success = false;
            result.ErrorMessage = "Error al procesar la solicitud. Por favor, intente más tarde.";
            return result;
        }
    }
}


