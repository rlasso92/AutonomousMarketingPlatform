using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Auth;
using AutonomousMarketingPlatform.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para autenticación y autorización.
/// </summary>
public class AccountController : Controller
{
    private readonly IMediator _mediator;
    private readonly SignInManager<Domain.Entities.ApplicationUser> _signInManager;
    private readonly UserManager<Domain.Entities.ApplicationUser> _userManager;
    private readonly ITenantResolverService _tenantResolver;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IMediator mediator,
        SignInManager<Domain.Entities.ApplicationUser> signInManager,
        UserManager<Domain.Entities.ApplicationUser> userManager,
        ITenantResolverService tenantResolver,
        ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _userManager = userManager;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    /// <summary>
    /// Muestra la página de login.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Si ya está autenticado, redirigir al dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        
        // Intentar obtener tenant del request
        var tenantId = await _tenantResolver.ResolveTenantIdAsync();
        ViewData["TenantId"] = tenantId;

        return View();
    }

    /// <summary>
    /// Procesa el login.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Resolver tenant
        var tenantId = await _tenantResolver.ResolveTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "No se pudo determinar el tenant. Por favor, use el header X-Tenant-Id o acceda desde el subdominio correcto.");
            return View(model);
        }

        // Ejecutar comando de login
        var command = new LoginCommand
        {
            Email = model.Email,
            Password = model.Password,
            TenantId = tenantId.Value,
            RememberMe = model.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            // Obtener usuario para agregar claims
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Agregar claim de TenantId
                var claims = new List<Claim>
                {
                    new Claim("TenantId", tenantId.Value.ToString()),
                    new Claim("FullName", user.FullName ?? user.Email ?? "")
                };

                // Agregar claims de roles para este tenant
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Agregar claims adicionales al usuario
                await _userManager.AddClaimsAsync(user, claims);
                
                await _signInManager.SignInAsync(user, model.RememberMe);
                
                _logger.LogInformation("Login exitoso: UserId={UserId}, Email={Email}, TenantId={TenantId}",
                    user.Id, user.Email, tenantId);

                return RedirectToLocal(returnUrl);
            }
        }

        // Si llegamos aquí, algo salió mal
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Cuenta bloqueada temporalmente.");
            ViewData["LockoutEnd"] = result.LockoutEnd;
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Intento de inicio de sesión no válido.");
            ViewData["RemainingAttempts"] = result.RemainingAttempts;
        }

        return View(model);
    }

    /// <summary>
    /// Procesa el logout.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = User.FindFirstValue("TenantId");

        await _signInManager.SignOutAsync();

        _logger.LogInformation("Logout: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

        return RedirectToAction("Login", "Account");
    }

    /// <summary>
    /// Página de acceso denegado.
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
