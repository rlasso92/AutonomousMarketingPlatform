using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using AutonomousMarketingPlatform.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para gestión de campañas.
/// </summary>
[Authorize]
public class CampaignsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<CampaignsController> _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public CampaignsController(
        IMediator mediator, 
        ILogger<CampaignsController> logger,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Lista todas las campañas del tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? status = null, Guid? tenantId = null)
    {
        try
        {
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            var currentTenantId = UserHelper.GetTenantId(User);
            
            Guid effectiveTenantId;
            
            // Si es SuperAdmin, permitir seleccionar tenant
            if (isSuperAdmin)
            {
                // Cargar lista de tenants para el selector
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
                ViewBag.IsSuperAdmin = true;
                
                // Si se proporciona un tenantId, usarlo; si no, usar Guid.Empty para ver todas
                effectiveTenantId = tenantId ?? Guid.Empty;
                ViewBag.SelectedTenantId = effectiveTenantId;
            }
            else
            {
                // Usuario normal: usar su TenantId
                if (!currentTenantId.HasValue)
                {
                    _logger.LogWarning("Usuario sin TenantId intentando acceder a campañas");
                    return RedirectToAction("Login", "Account");
                }
                effectiveTenantId = currentTenantId.Value;
            }

            _logger.LogInformation("Listando campañas para TenantId={TenantId}, Status={Status}, IsSuperAdmin={IsSuperAdmin}", 
                effectiveTenantId, status ?? "Todos", isSuperAdmin);

            var query = new ListCampaignsQuery
            {
                TenantId = effectiveTenantId,
                Status = status,
                IsSuperAdmin = isSuperAdmin
            };

            var campaigns = await _mediator.Send(query);
            
            _logger.LogInformation("Se encontraron {Count} campañas para TenantId={TenantId}", campaigns?.Count ?? 0, effectiveTenantId);
            
            ViewBag.StatusFilter = status;
            return View(campaigns ?? new List<CampaignListDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar campañas");
            ModelState.AddModelError(string.Empty, "Error al cargar las campañas. Por favor, intente nuevamente.");
            return View(new List<CampaignListDto>());
        }
    }

    /// <summary>
    /// Muestra el formulario para crear una nueva campaña.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Create(Guid? tenantId = null)
    {
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        
        // Si es SuperAdmin, cargar lista de tenants para seleccionar
        if (isSuperAdmin)
        {
            var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
            var tenants = await _mediator.Send(tenantsQuery);
            ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
            ViewBag.IsSuperAdmin = true;
            ViewBag.SelectedTenantId = tenantId;
        }
        
        return View(new CreateCampaignDto { Status = "Draft" });
    }

    /// <summary>
    /// Crea una nueva campaña.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Create([FromForm] CreateCampaignDto model, [FromForm] Guid? tenantId = null)
    {
        // Declarar isSuperAdmin al inicio del método para que esté disponible en todo el scope
        var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
        
        // SIEMPRE hacer binding manual de Objectives y TargetAudience
        // ASP.NET Core no bindea Dictionary<string, object> automáticamente
        if (model == null)
        {
            model = new CreateCampaignDto { Status = "Draft" };
        }
        
        // Binding manual de Objectives
        var objectivesGoals = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var key = $"Objectives[goals][{i}]";
            if (Request.Form.TryGetValue(key, out var goalValue) && !string.IsNullOrWhiteSpace(goalValue))
            {
                objectivesGoals.Add(goalValue.ToString());
            }
        }
        if (objectivesGoals.Count > 0)
        {
            model.Objectives = new Dictionary<string, object> { { "goals", objectivesGoals } };
        }
        
        // Binding manual de TargetAudience
        var targetAudience = new Dictionary<string, object>();
        if (Request.Form.TryGetValue("TargetAudience[ageRange]", out var ageRangeValue) && !string.IsNullOrWhiteSpace(ageRangeValue))
        {
            targetAudience["ageRange"] = ageRangeValue.ToString();
        }
        var interests = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var key = $"TargetAudience[interests][{i}]";
            if (Request.Form.TryGetValue(key, out var interestValue) && !string.IsNullOrWhiteSpace(interestValue))
            {
                interests.Add(interestValue.ToString());
            }
        }
        if (interests.Count > 0)
        {
            targetAudience["interests"] = interests;
        }
        if (targetAudience.Count > 0)
        {
            model.TargetAudience = targetAudience;
        }
        
        // Binding manual de TargetChannels
        if (Request.Form.TryGetValue("TargetChannels", out var channelsValue))
        {
            model.TargetChannels = channelsValue.ToList();
        }
        
        _logger.LogInformation("=== INICIO CREAR CAMPAÑA ===");
        _logger.LogInformation("Model es NULL? {IsNull}", model == null);
        
        // Validación: Name es obligatorio
        if (model != null && string.IsNullOrWhiteSpace(model.Name))
        {
            _logger.LogWarning("Name está vacío, agregando error a ModelState");
            ModelState.AddModelError("Name", "El nombre de la campaña es obligatorio.");
        }
        
        if (model != null)
        {
            _logger.LogInformation("=== DATOS DEL MODELO RECIBIDO ===");
            _logger.LogInformation("Name = '{Name}'", model.Name ?? "NULL");
            _logger.LogInformation("Status = '{Status}'", model.Status ?? "NULL");
            _logger.LogInformation("Description = '{Description}'", model.Description ?? "NULL");
            _logger.LogInformation("StartDate = {StartDate}", model.StartDate?.ToString() ?? "NULL");
            _logger.LogInformation("EndDate = {EndDate}", model.EndDate?.ToString() ?? "NULL");
            _logger.LogInformation("Budget = {Budget}", model.Budget?.ToString() ?? "NULL");
            _logger.LogInformation("Notes = '{Notes}'", model.Notes ?? "NULL");
            
            // Log de Objectives
            if (model.Objectives != null)
            {
                _logger.LogInformation("Objectives = {Objectives}", JsonSerializer.Serialize(model.Objectives));
                _logger.LogInformation("Objectives.Count = {Count}", model.Objectives.Count);
                foreach (var kvp in model.Objectives)
                {
                    _logger.LogInformation("  Objectives[{Key}] = {Value} (Type: {Type})", 
                        kvp.Key, kvp.Value, kvp.Value?.GetType().Name ?? "null");
                }
            }
            else
            {
                _logger.LogInformation("Objectives = NULL");
            }
            
            // Log de TargetAudience
            if (model.TargetAudience != null)
            {
                _logger.LogInformation("TargetAudience = {TargetAudience}", JsonSerializer.Serialize(model.TargetAudience));
                _logger.LogInformation("TargetAudience.Count = {Count}", model.TargetAudience.Count);
                foreach (var kvp in model.TargetAudience)
                {
                    _logger.LogInformation("  TargetAudience[{Key}] = {Value} (Type: {Type})", 
                        kvp.Key, kvp.Value, kvp.Value?.GetType().Name ?? "null");
                }
            }
            else
            {
                _logger.LogInformation("TargetAudience = NULL");
            }
            
            // Log de TargetChannels
            if (model.TargetChannels != null)
            {
                _logger.LogInformation("TargetChannels = {TargetChannels}", JsonSerializer.Serialize(model.TargetChannels));
                _logger.LogInformation("TargetChannels.Count = {Count}", model.TargetChannels.Count);
                for (int i = 0; i < model.TargetChannels.Count; i++)
                {
                    _logger.LogInformation("  TargetChannels[{Index}] = '{Value}'", i, model.TargetChannels[i]);
                }
            }
            else
            {
                _logger.LogInformation("TargetChannels = NULL");
            }
            
        }
        
        _logger.LogInformation("TenantId recibido: {TenantId}", tenantId);
        
        // Log de todos los datos del Request.Form
        _logger.LogInformation("=== REQUEST.FORM KEYS ===");
        _logger.LogInformation("Total keys: {Count}", Request.Form.Keys.Count);
        foreach (var key in Request.Form.Keys)
        {
            var values = Request.Form[key];
            if (values.Count == 1)
            {
                _logger.LogInformation("  {Key} = '{Value}'", key, values[0].ToString());
            }
            else
            {
                _logger.LogInformation("  {Key} = [{Values}] (Count: {Count})", 
                    key, string.Join(", ", values.Select(v => $"'{v}'")), values.Count);
            }
        }
        
        if (model == null)
        {
            _logger.LogError("Model es NULL! Intentando binding manual...");
            _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("Request.Method: {Method}", Request.Method);
            _logger.LogInformation("Request.HasFormContentType: {HasFormContentType}", Request.HasFormContentType);
            
            // Intentar binding manual
            model = new CreateCampaignDto { Status = "Draft" };
            
            if (Request.HasFormContentType)
            {
                _logger.LogInformation("Intentando binding manual desde Request.Form...");
                if (Request.Form.TryGetValue("Name", out var nameValue))
                {
                    model.Name = nameValue.ToString();
                    _logger.LogInformation("Name bindeado: {Name}", model.Name);
                }
                if (Request.Form.TryGetValue("Description", out var descValue))
                {
                    model.Description = descValue.ToString();
                }
                if (Request.Form.TryGetValue("Status", out var statusValue))
                {
                    model.Status = statusValue.ToString();
                }
                if (Request.Form.TryGetValue("StartDate", out var startDateValue) && DateTime.TryParse(startDateValue, out var startDate))
                {
                    model.StartDate = startDate;
                }
                if (Request.Form.TryGetValue("EndDate", out var endDateValue) && DateTime.TryParse(endDateValue, out var endDate))
                {
                    model.EndDate = endDate;
                }
                if (Request.Form.TryGetValue("Budget", out var budgetValue) && decimal.TryParse(budgetValue, out var budget))
                {
                    model.Budget = budget;
                }
                if (Request.Form.TryGetValue("Notes", out var notesValue))
                {
                    model.Notes = notesValue.ToString();
                }
                
                // TargetChannels (múltiple)
                if (Request.Form.TryGetValue("TargetChannels", out var channelsValue2))
                {
                    model.TargetChannels = channelsValue2.ToList();
                }
                
                // Objectives y TargetAudience ya se bindearon al inicio del método
                // No es necesario bindearlos aquí de nuevo
                
                _logger.LogInformation("Model después de binding manual: Name={Name}, Status={Status}", model.Name, model.Status);
            }
            
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                _logger.LogError("Model sigue siendo inválido después de binding manual");
                ModelState.AddModelError(string.Empty, "Error: No se recibieron datos del formulario.");
                
                // Recargar tenants si es SuperAdmin (isSuperAdmin ya está declarado al inicio del método)
                if (isSuperAdmin)
                {
                    var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                    var tenants = await _mediator.Send(tenantsQuery);
                    ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                    ViewBag.IsSuperAdmin = true;
                    ViewBag.SelectedTenantId = tenantId;
                }
                
                return View(model);
            }
        }

        // Validar tenantId para SuperAdmin ANTES de ModelState.IsValid (isSuperAdmin ya está declarado al inicio)
        if (isSuperAdmin)
        {
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            {
                ModelState.AddModelError("tenantId", "Debe seleccionar un tenant para crear la campaña.");
                _logger.LogWarning("SuperAdmin intentando crear campaña sin seleccionar tenant");
            }
        }
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("ModelState inválido. Total de errores: {ErrorCount}", errors.Count);
            foreach (var error in errors)
            {
                _logger.LogWarning("  - Error: {Error}", error);
            }
            
            // Recargar tenants si es SuperAdmin
            if (isSuperAdmin)
            {
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                ViewBag.IsSuperAdmin = true;
                ViewBag.SelectedTenantId = tenantId;
            }
            
            return View(model);
        }

        try
        {
            var userId = UserHelper.GetUserId(User);
            var currentTenantId = UserHelper.GetTenantId(User);
            // isSuperAdmin ya está declarado arriba, no redeclarar
            
            _logger.LogInformation("Usuario autenticado: UserId={UserId}, CurrentTenantId={CurrentTenantId}, IsSuperAdmin={IsSuperAdmin}", 
                userId, currentTenantId, isSuperAdmin);
            
            Guid effectiveTenantId;
            
            // Si es SuperAdmin, debe seleccionar un tenant (ya validado arriba, pero verificamos de nuevo por seguridad)
            if (isSuperAdmin)
            {
                _logger.LogInformation("Usuario es SuperAdmin, verificando tenant seleccionado");
                _logger.LogInformation("tenantId recibido del formulario: {TenantId}", tenantId);
                
                // Esta validación ya se hizo arriba antes de ModelState.IsValid, pero la mantenemos por seguridad
                if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
                {
                    _logger.LogError("ERROR: SuperAdmin llegó aquí sin tenantId válido (esto no debería pasar)");
                    ModelState.AddModelError("tenantId", "Debe seleccionar un tenant para crear la campaña.");
                    
                    // Recargar tenants
                    var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                    var tenants = await _mediator.Send(tenantsQuery);
                    ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                    ViewBag.IsSuperAdmin = true;
                    ViewBag.SelectedTenantId = tenantId;
                    
                    return View(model);
                }
                effectiveTenantId = tenantId.Value;
                _logger.LogInformation("SuperAdmin usando TenantId seleccionado: {TenantId}", effectiveTenantId);
            }
            else
            {
                // Usuario normal: NO usar tenantId del formulario, usar el de los claims
                // Ignorar tenantId si viene del formulario (no debería venir)
                if (tenantId.HasValue && tenantId.Value != Guid.Empty)
                {
                    _logger.LogWarning("Usuario normal recibió tenantId del formulario ({TenantId}), ignorándolo y usando TenantId de claims", tenantId.Value);
                }
                
                if (!userId.HasValue || !currentTenantId.HasValue)
                {
                    _logger.LogError("Usuario no autenticado correctamente: UserId={UserId}, TenantId={TenantId}", userId, currentTenantId);
                    return RedirectToAction("Login", "Account");
                }
                effectiveTenantId = currentTenantId.Value;
                _logger.LogInformation("Usuario normal usando TenantId de claims: {TenantId}", effectiveTenantId);
            }

            _logger.LogInformation("Preparando comando CreateCampaignCommand: TenantId={TenantId}, UserId={UserId}", 
                effectiveTenantId, userId.Value);

            // ========================================
            // AQUÍ SE CREA EL COMANDO CON LOS DATOS
            // ========================================
            var command = new CreateCampaignCommand
            {
                TenantId = effectiveTenantId,
                UserId = userId.Value,
                Campaign = model
            };

            _logger.LogInformation("=== COMANDO CREADO - IMPRIMIENDO DATOS ===");
            _logger.LogInformation("Command.TenantId = {TenantId}", command.TenantId);
            _logger.LogInformation("Command.UserId = {UserId}", command.UserId);
            _logger.LogInformation("Command.Campaign.Name = '{Name}'", command.Campaign?.Name ?? "NULL");
            _logger.LogInformation("Command.Campaign.Description = '{Description}'", command.Campaign?.Description ?? "NULL");
            _logger.LogInformation("Command.Campaign.Status = '{Status}'", command.Campaign?.Status ?? "NULL");
            _logger.LogInformation("Command.Campaign.StartDate = {StartDate}", command.Campaign?.StartDate?.ToString() ?? "NULL");
            _logger.LogInformation("Command.Campaign.EndDate = {EndDate}", command.Campaign?.EndDate?.ToString() ?? "NULL");
            _logger.LogInformation("Command.Campaign.Budget = {Budget}", command.Campaign?.Budget?.ToString() ?? "NULL");
            _logger.LogInformation("Command.Campaign.Notes = '{Notes}'", command.Campaign?.Notes ?? "NULL");
            
            // Objectives
            if (command.Campaign?.Objectives != null)
            {
                _logger.LogInformation("Command.Campaign.Objectives = {Objectives}", JsonSerializer.Serialize(command.Campaign.Objectives));
                _logger.LogInformation("Command.Campaign.Objectives.Count = {Count}", command.Campaign.Objectives.Count);
            }
            else
            {
                _logger.LogWarning("⚠⚠⚠ Command.Campaign.Objectives = NULL ⚠⚠⚠");
            }
            
            // TargetAudience
            if (command.Campaign?.TargetAudience != null)
            {
                _logger.LogInformation("Command.Campaign.TargetAudience = {TargetAudience}", JsonSerializer.Serialize(command.Campaign.TargetAudience));
                _logger.LogInformation("Command.Campaign.TargetAudience.Count = {Count}", command.Campaign.TargetAudience.Count);
            }
            else
            {
                _logger.LogWarning("⚠⚠⚠ Command.Campaign.TargetAudience = NULL ⚠⚠⚠");
            }
            
            // TargetChannels
            if (command.Campaign?.TargetChannels != null)
            {
                _logger.LogInformation("Command.Campaign.TargetChannels = {TargetChannels}", JsonSerializer.Serialize(command.Campaign.TargetChannels));
                _logger.LogInformation("Command.Campaign.TargetChannels.Count = {Count}", command.Campaign.TargetChannels.Count);
            }
            else
            {
                _logger.LogWarning("⚠⚠⚠ Command.Campaign.TargetChannels = NULL ⚠⚠⚠");
            }
            
            _logger.LogInformation("_mediator es NULL? {IsNull}", _mediator == null);
            
            // ========================================
            // AQUÍ SE ENVÍA EL COMANDO AL HANDLER
            // ========================================
            _logger.LogInformation("=== ENVIANDO COMANDO AL MEDIATOR (LÍNEA 515) ===");
            _logger.LogInformation("result = await _mediator.Send(command);");
            
            CampaignDetailDto result;
            try
            {
                result = await _mediator.Send(command);
                _logger.LogInformation("=== DESPUÉS DE LLAMAR AL MEDIATOR ===");
                _logger.LogInformation("Comando procesado exitosamente. CampaignId={CampaignId}", result.Id);
            }
            catch (Exception mediatorEx)
            {
                _logger.LogError("=== ERROR AL LLAMAR AL MEDIATOR ===");
                _logger.LogError("Tipo de excepción: {ExceptionType}", mediatorEx.GetType().Name);
                _logger.LogError("Mensaje: {Message}", mediatorEx.Message);
                _logger.LogError("Stack Trace: {StackTrace}", mediatorEx.StackTrace);
                if (mediatorEx.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerException}", mediatorEx.InnerException.Message);
                }
                throw;
            }

            _logger.LogInformation("=== CAMPAÑA CREADA EXITOSAMENTE ===");
            _logger.LogInformation("CampaignId={CampaignId}, Name={Name}, TenantId={TenantId}", 
                result.Id, result.Name, effectiveTenantId);
            
            TempData["SuccessMessage"] = "Campaña creada exitosamente.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogError("=== ERROR DE VALIDACIÓN ===");
            _logger.LogError("Total de errores: {ErrorCount}", ex.Errors.Count());
            foreach (var error in ex.Errors)
            {
                _logger.LogError("  - {PropertyName}: {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            _logger.LogError(ex, "Stack trace completo");
            
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            
            // Recargar tenants si es SuperAdmin (isSuperAdmin ya está declarado arriba)
            if (isSuperAdmin)
            {
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                ViewBag.IsSuperAdmin = true;
                ViewBag.SelectedTenantId = tenantId;
            }
            
            return View(model);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError("=== ERROR DE AUTORIZACIÓN ===");
            _logger.LogError("Mensaje: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace completo");
            TempData["ErrorMessage"] = $"Error de autorización: {ex.Message}";
            ModelState.AddModelError(string.Empty, ex.Message);
            
            // Recargar tenants si es SuperAdmin (isSuperAdmin ya está declarado arriba)
            if (isSuperAdmin)
            {
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                ViewBag.IsSuperAdmin = true;
                ViewBag.SelectedTenantId = tenantId;
            }
            
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError("=== ERROR INESPERADO AL CREAR CAMPAÑA ===");
            _logger.LogError("Tipo de excepción: {ExceptionType}", ex.GetType().Name);
            _logger.LogError("Mensaje: {Message}", ex.Message);
            _logger.LogError("Inner Exception: {InnerException}", ex.InnerException?.Message ?? "N/A");
            _logger.LogError(ex, "Stack trace completo");
            
            TempData["ErrorMessage"] = $"Error al crear la campaña: {ex.Message}";
            ModelState.AddModelError(string.Empty, "Error al crear la campaña. Por favor, intente nuevamente.");
            
            // Recargar tenants si es SuperAdmin (isSuperAdmin ya está declarado arriba)
            if (isSuperAdmin)
            {
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants.Where(t => t.IsActive).ToList();
                ViewBag.IsSuperAdmin = true;
                ViewBag.SelectedTenantId = tenantId;
            }
            
            return View(model);
        }
    }

    /// <summary>
    /// Muestra el detalle de una campaña.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var currentTenantId = UserHelper.GetTenantId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            
            // Para SuperAdmins, usar Guid.Empty para permitir ver campañas de cualquier tenant
            // Para usuarios normales, usar su TenantId
            var effectiveTenantId = isSuperAdmin ? Guid.Empty : (currentTenantId ?? Guid.Empty);
            
            if (!isSuperAdmin && !currentTenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetCampaignQuery
            {
                TenantId = effectiveTenantId,
                CampaignId = id,
                IsSuperAdmin = isSuperAdmin
            };

            var campaign = await _mediator.Send(query);

            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campaña {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Muestra el formulario para editar una campaña.
    /// </summary>
    [HttpGet]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var currentTenantId = UserHelper.GetTenantId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            
            // Para SuperAdmins, usar Guid.Empty para permitir editar campañas de cualquier tenant
            // Para usuarios normales, usar su TenantId
            var effectiveTenantId = isSuperAdmin ? Guid.Empty : (currentTenantId ?? Guid.Empty);
            
            if (!isSuperAdmin && !currentTenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = new GetCampaignQuery
            {
                TenantId = effectiveTenantId,
                CampaignId = id,
                IsSuperAdmin = isSuperAdmin
            };

            var campaign = await _mediator.Send(query);

            if (campaign == null)
            {
                _logger.LogWarning("Campaña {CampaignId} no encontrada para editar. TenantId: {TenantId}, IsSuperAdmin: {IsSuperAdmin}", 
                    id, effectiveTenantId, isSuperAdmin);
                TempData["ErrorMessage"] = "La campaña no fue encontrada o no tienes permisos para editarla.";
                return RedirectToAction("Index");
            }

            var updateDto = new UpdateCampaignDto
            {
                Name = campaign.Name,
                Description = campaign.Description,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Budget = campaign.Budget,
                Objectives = campaign.Objectives,
                TargetAudience = campaign.TargetAudience,
                TargetChannels = campaign.TargetChannels ?? new List<string>(),
                Notes = campaign.Notes
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campaña para editar {CampaignId}", id);
            TempData["ErrorMessage"] = "Error al cargar la campaña para editar. Por favor, intente nuevamente.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Actualiza una campaña existente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin", "Marketer")]
    public async Task<IActionResult> Edit(Guid id, UpdateCampaignDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = UserHelper.GetUserId(User);
            var currentTenantId = UserHelper.GetTenantId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Para SuperAdmins, necesitamos obtener el TenantId real de la campaña
            // Para usuarios normales, usar su TenantId
            Guid effectiveTenantId;
            if (isSuperAdmin)
            {
                // Obtener el TenantId real desde la base de datos usando el repositorio
                var campaignRepository = HttpContext.RequestServices.GetRequiredService<ICampaignRepository>();
                var campaignEntity = await campaignRepository.GetCampaignWithDetailsAsync(id, Guid.Empty);
                
                if (campaignEntity == null)
                {
                    TempData["ErrorMessage"] = "La campaña no fue encontrada.";
                    return RedirectToAction("Index");
                }
                
                effectiveTenantId = campaignEntity.TenantId;
            }
            else
            {
                if (!currentTenantId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }
                effectiveTenantId = currentTenantId.Value;
            }

            var command = new UpdateCampaignCommand
            {
                TenantId = effectiveTenantId,
                UserId = userId.Value,
                CampaignId = id,
                Campaign = model
            };

            var result = await _mediator.Send(command);

            TempData["SuccessMessage"] = "Campaña actualizada exitosamente.";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (NotFoundException)
        {
            TempData["ErrorMessage"] = "La campaña no fue encontrada.";
            return RedirectToAction("Index");
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar campaña {CampaignId}", id);
            ModelState.AddModelError(string.Empty, "Error al actualizar la campaña. Por favor, intente nuevamente.");
            return View(model);
        }
    }

    /// <summary>
    /// Elimina una campaña (soft delete).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AuthorizeRole("Owner", "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var command = new DeleteCampaignCommand
            {
                TenantId = tenantId.Value,
                UserId = userId.Value,
                CampaignId = id
            };

            await _mediator.Send(command);

            TempData["SuccessMessage"] = "Campaña eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar campaña {CampaignId}", id);
            TempData["ErrorMessage"] = "Error al eliminar la campaña. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Endpoint temporal de prueba para crear campaña y solicitar contenido (dispara n8n).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> TestCreateCampaignAndRequestContent()
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");

            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Si es SuperAdmin, necesitamos un tenant
            Guid effectiveTenantId;
            if (isSuperAdmin)
            {
                // Obtener el primer tenant activo
                var tenantsQuery = new Application.UseCases.Tenants.ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                var firstTenant = tenants.FirstOrDefault(t => t.IsActive);
                
                if (firstTenant == null)
                {
                    return Json(new { success = false, message = "No hay tenants disponibles para SuperAdmin" });
                }
                
                effectiveTenantId = firstTenant.Id;
            }
            else
            {
                if (!tenantId.HasValue)
                {
                    return Json(new { success = false, message = "TenantId no encontrado" });
                }
                effectiveTenantId = tenantId.Value;
            }

            // 1. Crear una campaña de prueba
            var campaignDto = new CreateCampaignDto
            {
                Name = $"Campaña de Prueba - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Description = "Campaña creada automáticamente para probar el flujo completo",
                Status = "Draft",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Budget = 1000,
                Objectives = new Dictionary<string, object>
                {
                    { "goals", new List<string> { "Aumentar engagement", "Generar leads" } }
                },
                TargetAudience = new Dictionary<string, object>
                {
                    { "ageRange", "18-35" },
                    { "interests", new[] { "tecnología", "marketing" } }
                },
                TargetChannels = new List<string> { "instagram", "facebook" }
            };

            var createCommand = new CreateCampaignCommand
            {
                TenantId = effectiveTenantId,
                UserId = userId.Value,
                Campaign = campaignDto
            };

            var createdCampaign = await _mediator.Send(createCommand);
            _logger.LogInformation("Campaña creada: {CampaignId}", createdCampaign.Id);

            // 2. Solicitar contenido para la campaña (dispara n8n)
            var automationService = HttpContext.RequestServices.GetRequiredService<IExternalAutomationService>();
            
            var eventData = new Dictionary<string, object>
            {
                { "instruction", "Crear contenido de marketing para Instagram promocionando la nueva campaña. Enfocarse en público joven, usar tono casual y moderno." },
                { "channels", new List<string> { "instagram" } },
                { "requiresApproval", false },
                { "assets", new List<string>() }
            };

            var requestId = await automationService.TriggerAutomationAsync(
                effectiveTenantId,
                "marketing.request",
                eventData,
                userId,
                createdCampaign.Id,
                null,
                CancellationToken.None);

            _logger.LogInformation("Solicitud de contenido enviada a n8n: RequestId={RequestId}", requestId);

            return Json(new
            {
                success = true,
                message = "Campaña creada y solicitud de contenido enviada a n8n exitosamente",
                campaignId = createdCampaign.Id,
                campaignName = createdCampaign.Name,
                requestId = requestId,
                tenantId = effectiveTenantId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en TestCreateCampaignAndRequestContent");
            return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Endpoint temporal de debug para verificar campañas en la BD.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DebugCampaigns()
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var currentTenantId = UserHelper.GetTenantId(User);
            var currentUserId = UserHelper.GetUserId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            
            // Mostrar TODAS las campañas (activas e inactivas) para debug
            var allCampaigns = await context.Campaigns
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.TenantId,
                    c.Status,
                    c.IsActive,
                    c.CreatedAt
                })
                .ToListAsync();
            
            // También contar totales
            var totalCampaigns = await context.Campaigns.CountAsync();
            var activeCampaigns = await context.Campaigns.CountAsync(c => c.IsActive);
            var inactiveCampaigns = totalCampaigns - activeCampaigns;

            var users = await context.Users
                .Select(u => new { u.Id, u.Email, u.TenantId })
                .Take(20)
                .ToListAsync();

            // Obtener todos los tenants únicos de las campañas
            var uniqueTenantIds = allCampaigns.Select(c => c.TenantId).Distinct().ToList();

            return Json(new
            {
                isSuperAdmin = isSuperAdmin,
                currentTenantId = currentTenantId?.ToString() ?? "null",
                currentUserId = currentUserId?.ToString() ?? "null",
                totalCampaignsInDb = totalCampaigns,
                activeCampaignsInDb = activeCampaigns,
                inactiveCampaignsInDb = inactiveCampaigns,
                campaignsReturned = allCampaigns.Count,
                uniqueTenantIds = uniqueTenantIds,
                campaigns = allCampaigns,
                users = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DebugCampaigns");
            return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

