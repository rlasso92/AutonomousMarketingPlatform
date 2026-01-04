using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Auth;
using AutonomousMarketingPlatform.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        try
        {
            Console.WriteLine($"[AccountController] Login GET iniciado - Path={HttpContext.Request.Path}, ReturnUrl={returnUrl}");
            _logger.LogInformation("=== AccountController.Login (GET) iniciado ===");
            _logger.LogInformation("ReturnUrl: {ReturnUrl}, Path: {Path}", returnUrl, HttpContext.Request.Path);
            
            // NO redirigir aquí - esto causa loops de redirección
            // La redirección solo debe ocurrir en el POST después de login exitoso
            
            ViewData["ReturnUrl"] = returnUrl;
            
            // Intentar obtener tenant del request (con manejo de errores)
            // IMPORTANTE: En Login, no es crítico tener tenant, puede ser super admin
            Guid? tenantId = null;
            try
            {
                Console.WriteLine("[AccountController] Intentando resolver tenant...");
                _logger.LogInformation("Intentando resolver tenant...");
                tenantId = await _tenantResolver.ResolveTenantIdAsync();
                Console.WriteLine($"[AccountController] Tenant resuelto: {tenantId?.ToString() ?? "NULL"}");
                _logger.LogInformation("Tenant resuelto: {TenantId}", tenantId?.ToString() ?? "NULL");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AccountController] WARNING: Error al resolver tenant: {ex.GetType().Name} - {ex.Message}");
                _logger.LogWarning(ex, "Error al resolver tenant, continuando sin tenant");
                // Continuar sin tenant (puede ser super admin o primera vez)
            }
            ViewData["TenantId"] = tenantId;

            // Crear modelo con valores por defecto para desarrollo
            var model = new LoginDto();
            try
            {
                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                {
                    model.Email = "admin@test.com";
                    model.Password = "Admin123!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener IWebHostEnvironment, continuando sin valores por defecto");
                // Continuar sin valores por defecto
            }

            Console.WriteLine("[AccountController] Login GET completado exitosamente, retornando View");
            _logger.LogInformation("=== AccountController.Login (GET) completado exitosamente ===");
            return View(model);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AccountController] ERROR en Login GET: {ex.GetType().Name}");
            Console.WriteLine($"[AccountController] Mensaje: {ex.Message}");
            Console.WriteLine($"[AccountController] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[AccountController] InnerException: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            }
            _logger.LogError(ex, "=== ERROR en AccountController.Login (GET) ===");
            _logger.LogError("Exception Type: {Type}, Message: {Message}, StackTrace: {StackTrace}", 
                ex.GetType().FullName, ex.Message, ex.StackTrace);
            throw; // Re-lanzar para que se vea el error real
        }
    }

    /// <summary>
    /// Procesa el login.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginDto model, [FromQuery] string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // Debug: Log de lo que está llegando
        _logger.LogWarning("Login POST recibido - Email: '{Email}', Password: '{Password}', Model null: {IsNull}", 
            model?.Email ?? "NULL", 
            string.IsNullOrEmpty(model?.Password) ? "EMPTY" : "***", 
            model == null);

        // Si el modelo es null, intentar leer desde Request.Form
        if (model == null)
        {
            _logger.LogWarning("Model es null, intentando leer desde Request.Form");
            model = new LoginDto
            {
                Email = Request.Form["Email"].ToString(),
                Password = Request.Form["Password"].ToString(),
                RememberMe = Request.Form["RememberMe"].ToString() == "true"
            };
            _logger.LogWarning("Valores desde Request.Form - Email: '{Email}', Password: '{Password}'", 
                model.Email, string.IsNullOrEmpty(model.Password) ? "EMPTY" : "***");
        }

        // Validar que el email y password no estén vacíos
        if (string.IsNullOrWhiteSpace(model?.Email))
        {
            ModelState.AddModelError(string.Empty, "El correo electrónico es requerido.");
            return View(model ?? new LoginDto());
        }

        if (string.IsNullOrWhiteSpace(model?.Password))
        {
            ModelState.AddModelError(string.Empty, "La contraseña es requerida.");
            return View(model ?? new LoginDto());
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Resolver tenant (opcional para super admin)
        var tenantIdNullable = await _tenantResolver.ResolveTenantIdAsync();
        Guid tenantId;
        Domain.Entities.ApplicationUser? user = null;
        
        // Si no hay tenant resuelto desde el request, intentar obtenerlo del usuario
        if (!tenantIdNullable.HasValue)
        {
            // Intentar encontrar el usuario para obtener su TenantId
            user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Si el usuario tiene un TenantId válido, usarlo
                if (user.TenantId != Guid.Empty)
                {
                    tenantId = user.TenantId;
                }
                else
                {
                    // Usuario con TenantId = Guid.Empty es super admin
                    var roles = await _userManager.GetRolesAsync(user);
                    var isSuperAdmin = roles.Contains("SuperAdmin") || roles.Contains("Owner");
                    
                    if (isSuperAdmin)
                    {
                        // Para super admin, usar Guid.Empty como TenantId temporal
                        tenantId = Guid.Empty;
                    }
                    else
                    {
                        // Usuario sin tenant y no es super admin
                        ModelState.AddModelError(string.Empty, "No se pudo determinar el tenant. Por favor, use el header X-Tenant-Id o acceda desde el subdominio correcto.");
                        return View(model);
                    }
                }
            }
            else
            {
                // Usuario no existe todavía, permitir intento de login
                // El LoginCommand validará las credenciales
                // Usar Guid.Empty temporalmente para permitir la validación
                tenantId = Guid.Empty;
            }
        }
        else
        {
            tenantId = tenantIdNullable.Value;
        }

        // Ejecutar comando de login
        var command = new LoginCommand
        {
            Email = model.Email,
            Password = model.Password,
            TenantId = tenantId,
            RememberMe = model.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            // Obtener usuario para agregar claims
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }
            
            if (user != null)
            {
                // Agregar claims
                var claims = new List<Claim>
                {
                    new Claim("FullName", user.FullName ?? user.Email ?? "")
                };
                
                // Agregar TenantId siempre (incluso si es Guid.Empty para super admin)
                claims.Add(new Claim("TenantId", tenantId.ToString()));
                
                // Si es super admin (tenantId == Guid.Empty), marcar como tal
                if (tenantId == Guid.Empty)
                {
                    claims.Add(new Claim("IsSuperAdmin", "true"));
                }

                // NO agregar claims de roles manualmente - Identity los maneja automáticamente
                // Los roles se incluyen automáticamente en los claims del usuario por Identity

                // Agregar claims adicionales al usuario
                _logger.LogInformation("=== INICIO LOGIN EXITOSO ===");
                _logger.LogInformation("Usuario: {Email}, UserId: {UserId}", user.Email, user.Id);
                _logger.LogInformation("TenantId del request: {TenantId}", tenantId);
                _logger.LogInformation("User.TenantId en DB: {UserTenantId}", user.TenantId);
                _logger.LogInformation("Es SuperAdmin: {IsSuperAdmin}", tenantId == Guid.Empty);
                
                // Obtener claims existentes
                var existingClaims = await _userManager.GetClaimsAsync(user);
                _logger.LogInformation("Claims existentes: {ExistingClaimsCount}", existingClaims.Count);
                
                // Remover claims personalizados duplicados (FullName, TenantId, IsSuperAdmin)
                // También remover claims de roles duplicados (Identity los maneja automáticamente desde AspNetUserRoles)
                var customClaimTypes = new[] { "FullName", "TenantId", "IsSuperAdmin" };
                var customClaimsToRemove = existingClaims.Where(c => customClaimTypes.Contains(c.Type)).ToList();
                
                // Remover claims de roles duplicados (Identity los genera automáticamente desde AspNetUserRoles)
                var roleClaimsToRemove = existingClaims.Where(c => c.Type == ClaimTypes.Role).ToList();
                
                if (customClaimsToRemove.Any() || roleClaimsToRemove.Any())
                {
                    var totalToRemove = customClaimsToRemove.Count + roleClaimsToRemove.Count;
                    _logger.LogInformation("Removiendo {Count} claims duplicados ({CustomCount} personalizados, {RoleCount} roles)", 
                        totalToRemove, customClaimsToRemove.Count, roleClaimsToRemove.Count);
                    
                    foreach (var claim in customClaimsToRemove)
                    {
                        await _userManager.RemoveClaimAsync(user, claim);
                    }
                    
                    foreach (var claim in roleClaimsToRemove)
                    {
                        await _userManager.RemoveClaimAsync(user, claim);
                    }
                    
                    _logger.LogInformation("Claims duplicados removidos exitosamente");
                }
                
                // Agregar los claims limpios
                _logger.LogInformation("Claims a agregar: {ClaimsCount}", claims.Count);
                foreach (var claim in claims)
                {
                    _logger.LogInformation("  - Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                await _userManager.AddClaimsAsync(user, claims);
                _logger.LogInformation("Claims agregados exitosamente");
                
                await _signInManager.SignInAsync(user, model.RememberMe);
                _logger.LogInformation("SignIn completado, RememberMe: {RememberMe}", model.RememberMe);
                
                // Verificar claims después del sign in
                var userAfterSignIn = await _signInManager.UserManager.GetUserAsync(User);
                if (userAfterSignIn != null)
                {
                    var claimsAfterSignIn = await _signInManager.UserManager.GetClaimsAsync(userAfterSignIn);
                    _logger.LogInformation("Claims después del SignIn: {ClaimsCount}", claimsAfterSignIn.Count);
                    foreach (var claim in claimsAfterSignIn)
                    {
                        _logger.LogInformation("  - Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }
                }
                
                _logger.LogInformation("RedirectToLocal con returnUrl: {ReturnUrl}", returnUrl ?? "null");
                _logger.LogInformation("=== FIN LOGIN EXITOSO ===");

                // Redirección inteligente según rol y returnUrl
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Redirigiendo a returnUrl: {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                // Redirigir al dashboard (Home/Index)
                _logger.LogInformation("Redirigiendo a Home/Index después de login exitoso");
                return RedirectToAction("Index", "Home");
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
    /// Procesa el logout (POST con antiforgery token).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        return await PerformLogout();
    }

    /// <summary>
    /// Logout GET (para compatibilidad con navegadores y enlaces directos).
    /// </summary>
    [HttpGet]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutGet()
    {
        return await PerformLogout();
    }

    private async Task<IActionResult> PerformLogout()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenantId = User.FindFirstValue("TenantId");

            _logger.LogInformation("Iniciando logout: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            await _signInManager.SignOutAsync();

            _logger.LogInformation("Logout completado exitosamente: UserId={UserId}, TenantId={TenantId}", userId, tenantId);

            // Limpiar cookies de sesión
            Response.Cookies.Delete("AutonomousMarketingPlatform.Auth");
            
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al hacer logout");
            // Aun así redirigir al login
            return RedirectToAction("Login", "Account");
        }
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
