using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.UseCases.Users;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
            var command = new CreateUserCommand
            {
                Email = model.Email,
                Password = model.Password,
                FullName = model.FullName,
                TenantId = model.TenantId,
                Role = model.Role,
                IsActive = model.IsActive
            };

            await _mediator.Send(command);
            TempData["SuccessMessage"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            ModelState.AddModelError("", ex.Message);
            
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
}

