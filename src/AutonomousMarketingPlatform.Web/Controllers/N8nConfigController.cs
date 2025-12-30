using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.N8n;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para configurar n8n desde el frontend.
/// Requiere rol Owner o Admin.
/// </summary>
[Authorize]
[AuthorizeRole("Owner", "Admin", "SuperAdmin")]
public class N8nConfigController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<N8nConfigController> _logger;

    public N8nConfigController(IMediator mediator, ILogger<N8nConfigController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Vista principal para configurar n8n.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? tenantId = null)
    {
        try
        {
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            var currentTenantId = UserHelper.GetTenantId(User);
            
            // Si es SuperAdmin, permitir seleccionar tenant
            if (isSuperAdmin)
            {
                // Cargar lista de tenants para el selector
                var tenantsQuery = new ListTenantsQuery();
                var tenants = await _mediator.Send(tenantsQuery);
                ViewBag.Tenants = tenants;
                ViewBag.IsSuperAdmin = true;
                
                // Si se proporciona un tenantId, usarlo; si no, usar el primero disponible
                var selectedTenantId = tenantId ?? (tenants.FirstOrDefault()?.Id ?? Guid.Empty);
                
                if (selectedTenantId == Guid.Empty && tenants.Any())
                {
                    selectedTenantId = tenants.First().Id;
                }
                
                // Si aún no hay tenant seleccionado, redirigir con el primer tenant
                if (selectedTenantId == Guid.Empty && tenants.Any())
                {
                    return RedirectToAction(nameof(Index), new { tenantId = tenants.First().Id });
                }
                
                var query = new GetN8nConfigQuery
                {
                    TenantId = selectedTenantId,
                    IsSuperAdmin = true // Permitir acceso a cualquier tenant
                };
                var config = await _mediator.Send(query);
                ViewBag.SelectedTenantId = selectedTenantId;
                
                _logger.LogInformation("Cargando configuración de n8n para SuperAdmin. TenantId={TenantId}, BaseUrl={BaseUrl}", 
                    selectedTenantId, config.BaseUrl);
                
                return View(config);
            }
            
            // Usuario normal: usar su tenant
            if (!currentTenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var normalQuery = new GetN8nConfigQuery
            {
                TenantId = currentTenantId.Value
            };
            var normalConfig = await _mediator.Send(normalQuery);
            ViewBag.IsSuperAdmin = false;
            return View(normalConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar configuración de n8n");
            return View(new N8nConfigDto());
        }
    }

    /// <summary>
    /// Endpoint para guardar/actualizar configuración de n8n.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] UpdateN8nConfigDto dto, Guid? tenantId = null)
    {
        try
        {
            var userId = UserHelper.GetUserId(User);
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            var currentTenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue)
            {
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });
            }

            // Determinar el tenant a usar
            Guid targetTenantId;
            if (isSuperAdmin && tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                // SuperAdmin puede especificar cualquier tenant
                targetTenantId = tenantId.Value;
            }
            else if (currentTenantId.HasValue)
            {
                // Usuario normal usa su tenant
                targetTenantId = currentTenantId.Value;
            }
            else
            {
                return Unauthorized(new { error = "No se pudo determinar el tenant" });
            }

            var command = new UpdateN8nConfigCommand
            {
                TenantId = targetTenantId,
                UserId = userId.Value,
                Config = dto,
                IsSuperAdmin = isSuperAdmin // Permitir actualizar cualquier tenant si es SuperAdmin
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                TempData["SuccessMessage"] = $"Configuración de n8n guardada correctamente para el tenant seleccionado.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo actualizar la configuración.";
            }

            // Redirigir con el tenantId si es SuperAdmin
            if (isSuperAdmin && tenantId.HasValue)
            {
                return RedirectToAction(nameof(Index), new { tenantId = tenantId.Value });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar configuración de n8n");
            TempData["ErrorMessage"] = "Error al guardar la configuración. Por favor, intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Endpoint para probar la conexión con n8n.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestConnection([FromBody] TestN8nConnectionDto? request = null)
    {
        try
        {
            var tenantId = UserHelper.GetTenantId(User);
            
            // Si no se proporciona request, usar la configuración de la BD
            if (request == null && tenantId.HasValue)
            {
                var configQuery = new GetN8nConfigQuery
                {
                    TenantId = tenantId.Value,
                    IsSuperAdmin = User.HasClaim("IsSuperAdmin", "true")
                };
                var config = await _mediator.Send(configQuery);
                
                request = new TestN8nConnectionDto
                {
                    BaseUrl = config.BaseUrl,
                    ApiKey = config.ApiKey
                };
            }
            
            // Si aún no hay request, usar valores por defecto
            if (request == null)
            {
                request = new TestN8nConnectionDto
                {
                    BaseUrl = "https://n8n.bashpty.com"
                };
            }

            var command = new TestN8nConnectionCommand
            {
                Request = request
            };

            var result = await _mediator.Send(command);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión con n8n");
            return Json(new TestN8nConnectionResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Endpoint para probar el webhook de marketing request.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestWebhook([FromBody] TestWebhookRequest request)
    {
        try
        {
            var isSuperAdmin = User.HasClaim("IsSuperAdmin", "true");
            var tenantId = UserHelper.GetTenantId(User);
            var userId = UserHelper.GetUserId(User);
            
            // Si es SuperAdmin y el request incluye un tenantId, usarlo
            if (isSuperAdmin && request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
            {
                tenantId = request.TenantId;
                _logger.LogInformation(
                    "SuperAdmin probando webhook para Tenant {TenantId} (especificado en request)",
                    tenantId);
            }
            
            if (!tenantId.HasValue || !userId.HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado correctamente o TenantId no especificado" });
            }

            // Usar el servicio de automatización para disparar el webhook
            var automationService = HttpContext.RequestServices.GetRequiredService<IExternalAutomationService>();
            
            // Crear eventData como Dictionary para marketing.request
            // IMPORTANTE: Las claves deben estar en minúsculas como espera n8n
            var eventData = new Dictionary<string, object>
            {
                { "instruction", request.Instruction ?? "Prueba de webhook desde frontend - Crear contenido de marketing para Instagram" },
                { "channels", request.Channels ?? new List<string> { "instagram" } },
                { "requiresApproval", request.RequiresApproval ?? false },
                { "assets", request.Assets ?? new List<string>() }
            };

            var requestId = await automationService.TriggerAutomationAsync(
                tenantId.Value,
                "marketing.request",
                eventData,
                userId,
                null,
                null,
                CancellationToken.None);

            return Json(new 
            { 
                success = true, 
                message = "Webhook disparado correctamente",
                requestId = requestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar webhook");
            return Json(new 
            { 
                success = false, 
                message = $"Error: {ex.Message}",
                error = ex.ToString()
            });
        }
    }

    /// <summary>
    /// Endpoint para obtener información de workflows.
    /// </summary>
    [HttpGet]
    public IActionResult GetWorkflowsInfo()
    {
        var workflows = new List<N8nWorkflowInfo>
        {
            new N8nWorkflowInfo
            {
                Name = "Trigger - Marketing Request",
                Description = "Recibe y valida solicitudes de marketing desde el backend",
                EventType = "marketing.request",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Validate Consents",
                Description = "Valida consentimientos de IA y publicación",
                EventType = "validate.consents",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Load Marketing Memory",
                Description = "Carga contexto histórico del tenant",
                EventType = "load.memory",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Analyze Instruction AI",
                Description = "Analiza instrucciones del usuario usando IA",
                EventType = "analyze.instruction",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Generate Marketing Strategy",
                Description = "Genera estrategia de marketing",
                EventType = "generate.strategy",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Generate Marketing Copy",
                Description = "Genera copy de marketing (corto, largo, hashtags)",
                EventType = "generate.copy",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Generate Visual Prompts",
                Description = "Genera prompts para IA visual",
                EventType = "generate.visual.prompts",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Build Marketing Pack",
                Description = "Construye el pack completo de marketing",
                EventType = "build.marketing.pack",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Human Approval Flow",
                Description = "Maneja el flujo de aprobación humana",
                EventType = "human.approval",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Publish Content",
                Description = "Publica contenido en redes sociales",
                EventType = "publish.content",
                Status = "Unknown"
            },
            new N8nWorkflowInfo
            {
                Name = "Metrics & Learning",
                Description = "Guarda métricas y aprendizaje",
                EventType = "metrics.learning",
                Status = "Unknown"
            }
        };

        return Json(workflows);
    }
}

/// <summary>
/// Request DTO para probar webhook.
/// </summary>
public class TestWebhookRequest
{
    public string? Instruction { get; set; }
    public List<string>? Channels { get; set; }
    public bool? RequiresApproval { get; set; }
    public List<string>? Assets { get; set; }
    /// <summary>
    /// TenantId opcional para SuperAdmins (permite especificar el tenant a probar).
    /// </summary>
    public Guid? TenantId { get; set; }
}
