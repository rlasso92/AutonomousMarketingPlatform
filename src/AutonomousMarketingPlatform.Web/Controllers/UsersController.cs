using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.Users;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de usuarios.
/// Requiere rol Admin, Owner o SuperAdmin.
/// </summary>
[Authorize]
public class UsersController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    private readonly ILoggingService _loggingService;

    public UsersController(
        IMediator mediator, 
        ILogger<UsersController> logger,
        ILoggingService loggingService)
    {
        _mediator = mediator;
        _logger = logger;
        _loggingService = loggingService;
    }

    /// <summary>
    /// Lista de usuarios.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            
            var query = new ListUsersQuery
            {
                TenantId = isSuperAdmin ? Guid.Empty : (tenantId ?? Guid.Empty)
            };

            var users = await _mediator.Send(query);
            return View(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar usuarios");
            return View(new List<UserListDto>());
        }
    }

    /// <summary>
    /// Formulario para crear usuario.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var model = new CreateUserDto();
        
        // Si es SuperAdmin, cargar lista de tenants para selección
        if (isSuperAdmin)
        {
            var tenantsQuery = new ListTenantsQuery();
            var tenants = await _mediator.Send(tenantsQuery);
            ViewBag.Tenants = tenants;
        }
        else
        {
            // Si no es SuperAdmin, usar el tenant del usuario actual
            var tenantId = UserHelper.GetTenantId(User);
            if (tenantId.HasValue)
            {
                model.TenantId = tenantId.Value;
            }
        }
        
        return View(model);
    }

    /// <summary>
    /// Crear usuario.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        _logger.LogInformation("[UsersController.Create] Iniciando creación de usuario. Email: {Email}, TenantId: {TenantId}", 
            model.Email ?? "NULL", model.TenantId);
        
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            _logger.LogWarning("[UsersController.Create] Acceso denegado. Usuario no tiene permisos");
            return RedirectToAction("AccessDenied", "Account");
        }

        // Si no es SuperAdmin, obtener TenantId del usuario actual
        if (!isSuperAdmin)
        {
            var tenantId = UserHelper.GetTenantId(User);
            if (tenantId.HasValue)
            {
                model.TenantId = tenantId.Value;
                _logger.LogInformation("[UsersController.Create] TenantId asignado desde usuario actual: {TenantId}", tenantId.Value);
            }
        }

        // Validar que el email no esté vacío
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            _logger.LogWarning("[UsersController.Create] Email vacío");
            ModelState.AddModelError(nameof(model.Email), "El email es requerido");
        }

        // Validar que la contraseña no esté vacía
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            _logger.LogWarning("[UsersController.Create] Contraseña vacía");
            ModelState.AddModelError(nameof(model.Password), "La contraseña es requerida");
        }
        else
        {
            // Validar que las contraseñas coincidan
            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("[UsersController.Create] Las contraseñas no coinciden");
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Las contraseñas no coinciden");
            }
            
            // Validar requisitos de contraseña
            if (model.Password.Length < 8)
            {
                _logger.LogWarning("[UsersController.Create] Contraseña muy corta: {Length} caracteres", model.Password.Length);
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe tener al menos 8 caracteres");
            }
            if (!model.Password.Any(char.IsUpper))
            {
                _logger.LogWarning("[UsersController.Create] Contraseña sin mayúsculas");
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe contener al menos una letra mayúscula");
            }
            if (!model.Password.Any(char.IsLower))
            {
                _logger.LogWarning("[UsersController.Create] Contraseña sin minúsculas");
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe contener al menos una letra minúscula");
            }
            if (!model.Password.Any(char.IsDigit))
            {
                _logger.LogWarning("[UsersController.Create] Contraseña sin números");
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe contener al menos un número");
            }
            if (!model.Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                _logger.LogWarning("[UsersController.Create] Contraseña sin caracteres especiales");
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe contener al menos un carácter especial");
            }
        }

        // Validar que el tenant no sea Guid.Empty (solo si es SuperAdmin)
        if (isSuperAdmin && model.TenantId == Guid.Empty)
        {
            _logger.LogWarning("[UsersController.Create] TenantId vacío para SuperAdmin");
            ModelState.AddModelError(nameof(model.TenantId), "Debe seleccionar un tenant");
        }
        else if (!isSuperAdmin && model.TenantId == Guid.Empty)
        {
            _logger.LogError("[UsersController.Create] TenantId vacío para usuario no SuperAdmin");
            ModelState.AddModelError("", "No se pudo determinar el tenant. Por favor, contacte al administrador.");
        }

        // Si hay errores de validación, retornar la vista
        if (!ModelState.IsValid)
        {
            // Loggear TODOS los errores de ModelState con detalles
            var errors = ModelState
                .Where(x => x.Value?.Errors != null && x.Value.Errors.Any())
                .SelectMany(x => x.Value.Errors.Select(e => new { Field = x.Key, Error = e.ErrorMessage }))
                .ToList();
            
            var validationErrors = string.Join(", ", errors.Select(e => $"{e.Field}: {e.Error}"));
            _logger.LogWarning("[UsersController.Create] Validación fallida. Errores: {Errors}", validationErrors);
            _logger.LogWarning("[UsersController.Create] Model recibido - Email: {Email}, TenantId: {TenantId}, Role: {Role}, IsActive: {IsActive}, PasswordLength: {PasswordLength}, ConfirmPasswordLength: {ConfirmPasswordLength}, FullName: {FullName}",
                model.Email ?? "NULL",
                model.TenantId,
                model.Role ?? "NULL",
                model.IsActive,
                model.Password?.Length ?? 0,
                model.ConfirmPassword?.Length ?? 0,
                model.FullName ?? "NULL");
            
            // Guardar error en base de datos con detalles completos
            await SaveErrorToDatabase(
                "HTTP 400 - Validación fallida al crear usuario",
                validationErrors,
                null,
                model);
            
            // Recargar tenants si es SuperAdmin
            if (isSuperAdmin)
            {
                var tenantsQuery = new ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
            }
            return View(model);
        }

        try
        {
            _logger.LogInformation("[UsersController.Create] Validación exitosa, creando usuario...");

            var command = new CreateUserCommand
            {
                Email = model.Email.Trim(),
                Password = model.Password,
                FullName = model.FullName.Trim(),
                TenantId = model.TenantId,
                Role = model.Role,
                IsActive = model.IsActive
            };

            await _mediator.Send(command);
            _logger.LogInformation("[UsersController.Create] Usuario creado exitosamente. Email: {Email}", model.Email);
            TempData["SuccessMessage"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[UsersController.Create] Error al crear usuario: {Message}", ex.Message);
            
            // Guardar error en base de datos
            await SaveErrorToDatabase(
                "HTTP 400 - InvalidOperationException al crear usuario",
                ex.Message,
                ex,
                model);
            
            // Detectar si es error de usuario existente
            if (ex.Message.Contains("ya existe") || ex.Message.Contains("already exists"))
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con este email. Por favor, use otro email.");
            }
            else if (ex.Message.Contains("Tenant") && ex.Message.Contains("no encontrado"))
            {
                ModelState.AddModelError(nameof(model.TenantId), "El tenant seleccionado no existe");
            }
            else if (ex.Message.Contains("Rol") && ex.Message.Contains("no encontrado"))
            {
                ModelState.AddModelError(nameof(model.Role), "El rol seleccionado no existe");
            }
            else if (ex.Message.Contains("Error al crear usuario"))
            {
                // Extraer errores de Identity
                var errorParts = ex.Message.Split(':');
                if (errorParts.Length > 1)
                {
                    ModelState.AddModelError(nameof(model.Password), errorParts[1].Trim());
                }
                else
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", ex.Message);
            }
            
            // Recargar tenants si es SuperAdmin
            if (isSuperAdmin)
            {
                var tenantsQuery = new ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
            }
            
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UsersController.Create] Error inesperado al crear usuario. StackTrace: {StackTrace}", ex.StackTrace);
            
            // Guardar error en base de datos
            await SaveErrorToDatabase(
                "HTTP 400 - Error inesperado al crear usuario",
                ex.Message,
                ex,
                model);
            
            ModelState.AddModelError("", "Ocurrió un error inesperado al crear el usuario. Por favor, intente nuevamente.");
            
            // Recargar tenants si es SuperAdmin
            if (isSuperAdmin)
            {
                var tenantsQuery = new ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
            }
            
            return View(model);
        }
    }

    /// <summary>
    /// Detalles de usuario.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var query = new GetUserQuery
            {
                UserId = id
            };

            var user = await _mediator.Send(query);
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles del usuario {UserId}", id);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Muestra el formulario para editar un usuario.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var query = new GetUserQuery
            {
                UserId = id
            };

            var user = await _mediator.Send(query);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateUserDto
            {
                FullName = user.FullName,
                IsActive = user.IsActive,
                Role = user.Roles.FirstOrDefault()
            };

            ViewBag.UserId = id;
            ViewBag.Email = user.Email;
            ViewBag.TenantName = user.TenantName;
            
            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario para editar {UserId}", id);
            TempData["ErrorMessage"] = "Error al cargar el usuario para editar.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateUserDto model)
    {
        // Verificar permisos: Admin, Owner o SuperAdmin
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        var isAdmin = User.IsInRole("Admin");
        var isOwner = User.IsInRole("Owner");
        
        if (!isSuperAdmin && !isAdmin && !isOwner)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (!ModelState.IsValid)
        {
            // Recargar datos del usuario
            try
            {
                var getUserQuery = new GetUserQuery { UserId = id };
                var user = await _mediator.Send(getUserQuery);
                ViewBag.UserId = id;
                ViewBag.Email = user?.Email ?? "";
                ViewBag.TenantName = user?.TenantName ?? "";
            }
            catch { }
            
            return View(model);
        }

        try
        {
            var command = new UpdateUserCommand
            {
                UserId = id,
                FullName = model.FullName,
                IsActive = model.IsActive,
                Role = model.Role
            };

            await _mediator.Send(command);
            TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UserId}", id);
            ModelState.AddModelError("", ex.Message);
            
            // Recargar datos del usuario
            try
            {
                var getUserQuery = new GetUserQuery { UserId = id };
                var user = await _mediator.Send(getUserQuery);
                ViewBag.UserId = id;
                ViewBag.Email = user?.Email ?? "";
                ViewBag.TenantName = user?.TenantName ?? "";
            }
            catch { }
            
            return View(model);
        }
    }

    /// <summary>
    /// Activar/Desactivar usuario.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        try
        {
            // Obtener usuario actual para saber su estado
            var getUserQuery = new GetUserQuery
            {
                UserId = id
            };

            var currentUser = await _mediator.Send(getUserQuery);
            
            // Cambiar el estado (toggle)
            var command = new UpdateUserCommand
            {
                UserId = id,
                IsActive = !currentUser.IsActive
            };

            await _mediator.Send(command);
            TempData["SuccessMessage"] = $"Usuario {(command.IsActive.Value ? "activado" : "desactivado")} exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Guarda un error en la base de datos ApplicationLogs.
    /// </summary>
    private async Task SaveErrorToDatabase(
        string errorTitle,
        string errorMessage,
        Exception? exception,
        CreateUserDto? model = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            var userId = UserHelper.GetUserId(User);
            var requestId = HttpContext.TraceIdentifier;
            var path = HttpContext.Request.Path.ToString();
            var httpMethod = HttpContext.Request.Method;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Preparar datos adicionales del modelo (sin contraseña por seguridad)
            string? additionalData = null;
            if (model != null)
            {
                var modelData = new
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,
                    TenantId = model.TenantId,
                    IsActive = model.IsActive,
                    HasPassword = !string.IsNullOrEmpty(model.Password),
                    PasswordLength = model.Password?.Length ?? 0,
                    HasConfirmPassword = !string.IsNullOrEmpty(model.ConfirmPassword),
                    PasswordsMatch = model.Password == model.ConfirmPassword
                };
                additionalData = JsonSerializer.Serialize(modelData);
            }

            var fullMessage = $"{errorTitle}: {errorMessage}";
            if (model != null)
            {
                fullMessage += $" | Email: {model.Email ?? "NULL"} | TenantId: {model.TenantId}";
            }

            await _loggingService.LogErrorAsync(
                message: fullMessage,
                source: "UsersController.Create",
                exception: exception,
                tenantId: tenantId,
                userId: userId,
                requestId: requestId,
                path: path,
                httpMethod: httpMethod,
                additionalData: additionalData,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        catch (Exception ex)
        {
            // Si falla al guardar el log, al menos registrarlo en el logger estándar
            _logger.LogError(ex, "[UsersController.SaveErrorToDatabase] Error al guardar log en base de datos. Error original: {OriginalError}", errorMessage);
        }
    }
}

