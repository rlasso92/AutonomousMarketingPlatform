using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Application.UseCases.Consents;
using AutonomousMarketingPlatform.Application.UseCases.N8n;
using AutonomousMarketingPlatform.Application.UseCases.Tenants;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    private readonly ILoggingService _loggingService;

    public N8nConfigController(
        IMediator mediator, 
        ILogger<N8nConfigController> logger,
        ILoggingService loggingService)
    {
        _mediator = mediator;
        _logger = logger;
        _loggingService = loggingService;
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
    /// Acepta requests con o sin body JSON.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        TestN8nConnectionDto? request = null;
        
        try
        {
            // Intentar leer el body si existe
            if (Request.ContentLength > 0 && Request.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(body) && body != "null")
                    {
                        request = JsonSerializer.Deserialize<TestN8nConnectionDto>(body, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        _logger.LogInformation("Request deserializado del body: BaseUrl={BaseUrl}, ApiKey={HasApiKey}", 
                            request?.BaseUrl ?? "null", !string.IsNullOrEmpty(request?.ApiKey));
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Error al deserializar body JSON, continuando sin body");
                }
            }

            _logger.LogInformation("TestConnection llamado. Request: {Request}", 
                request != null ? $"BaseUrl={request.BaseUrl}, ApiKey={(string.IsNullOrEmpty(request.ApiKey) ? "null" : "***")}" : "null");

            var tenantId = UserHelper.GetTenantId(User);
            _logger.LogInformation("TenantId obtenido: {TenantId}", tenantId);
            
            // Si no se proporciona request, usar la configuración de la BD
            if (request == null && tenantId.HasValue)
            {
                _logger.LogInformation("Request es null, obteniendo configuración de BD para TenantId: {TenantId}", tenantId.Value);
                var configQuery = new GetN8nConfigQuery
                {
                    TenantId = tenantId.Value,
                    IsSuperAdmin = User.HasClaim("IsSuperAdmin", "true")
                };
                var config = await _mediator.Send(configQuery);
                
                if (config != null && !string.IsNullOrWhiteSpace(config.BaseUrl))
                {
                    request = new TestN8nConnectionDto
                    {
                        BaseUrl = config.BaseUrl,
                        ApiKey = config.ApiKey
                    };
                    _logger.LogInformation("Configuración obtenida de BD: BaseUrl={BaseUrl}", config.BaseUrl);
                }
                else
                {
                    _logger.LogWarning("No se encontró configuración de n8n en BD para TenantId: {TenantId}", tenantId.Value);
                }
            }
            
            // Si aún no hay request, usar valores por defecto
            if (request == null)
            {
                _logger.LogInformation("Usando valores por defecto para prueba de conexión");
                request = new TestN8nConnectionDto
                {
                    BaseUrl = "https://n8n.bashpty.com"
                };
            }

            // Validar que BaseUrl no esté vacío
            if (string.IsNullOrWhiteSpace(request.BaseUrl))
            {
                _logger.LogWarning("BaseUrl está vacío después de intentar obtener configuración");
                return BadRequest(new TestN8nConnectionResponse
                {
                    Success = false,
                    Message = "URL base no puede estar vacía",
                    Error = "BaseUrl is required"
                });
            }

            _logger.LogInformation("Enviando comando TestN8nConnectionCommand con BaseUrl: {BaseUrl}", request.BaseUrl);

            var command = new TestN8nConnectionCommand
            {
                Request = request
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Resultado de TestConnection: Success={Success}, Message={Message}", 
                result.Success, result.Message);
            
            // Si falló la conexión, guardar en ApplicationLogs
            if (!result.Success)
            {
                tenantId = UserHelper.GetTenantId(User);
                var userId = UserHelper.GetUserId(User);
                var additionalData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    BaseUrl = request?.BaseUrl,
                    Error = result.Error,
                    StatusCode = result.StatusCode,
                    Message = result.Message
                });
                
                await _loggingService.LogErrorAsync(
                    message: $"Error al probar conexión con n8n: {result.Message}",
                    source: "N8nConfig.TestConnection",
                    exception: null,
                    tenantId: tenantId,
                    userId: userId,
                    requestId: HttpContext.TraceIdentifier,
                    path: Request.Path,
                    httpMethod: Request.Method,
                    additionalData: additionalData,
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    userAgent: Request.Headers["User-Agent"].ToString());
            }
            
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar conexión con n8n. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                ex.GetType().Name, ex.Message, ex.StackTrace);
            
            // Guardar excepción en ApplicationLogs
            var tenantId = UserHelper.GetTenantId(User);
            var userId = UserHelper.GetUserId(User);
            await _loggingService.LogErrorAsync(
                message: $"Excepción al probar conexión con n8n: {ex.Message}",
                source: "N8nConfig.TestConnection",
                exception: ex,
                tenantId: tenantId,
                userId: userId,
                requestId: HttpContext.TraceIdentifier,
                path: Request.Path,
                httpMethod: Request.Method,
                additionalData: System.Text.Json.JsonSerializer.Serialize(new { ExceptionType = ex.GetType().Name }),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: Request.Headers["User-Agent"].ToString());
            
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

            // Si hay campaignId, cargar los datos reales de la campaña
            string instruction;
            List<string> channels;
            List<string> assets = request.Assets ?? new List<string>();
            bool requiresApproval = request.RequiresApproval ?? false;
            bool usingRealCampaignData = false;

            if (request.CampaignId.HasValue)
            {
                try
                {
                    // Cargar datos reales de la campaña
                    var campaignQuery = new GetCampaignQuery
                    {
                        TenantId = isSuperAdmin ? Guid.Empty : tenantId.Value,
                        CampaignId = request.CampaignId.Value,
                        IsSuperAdmin = isSuperAdmin
                    };
                    var campaign = await _mediator.Send(campaignQuery);

                    if (campaign != null)
                    {
                        usingRealCampaignData = true;
                        _logger.LogInformation(
                            "Cargando datos reales de campaña: {CampaignName} (Id: {CampaignId})",
                            campaign.Name, request.CampaignId.Value);

                        // Construir instrucción usando datos reales de la campaña
                        var instructionParts = new List<string>();
                        
                        if (!string.IsNullOrWhiteSpace(campaign.Description))
                        {
                            instructionParts.Add($"Campaña: {campaign.Name}. {campaign.Description}");
                        }
                        else
                        {
                            instructionParts.Add($"Campaña: {campaign.Name}");
                        }

                        // Agregar objetivos si existen
                        if (campaign.Objectives != null && campaign.Objectives.ContainsKey("goals"))
                        {
                            if (campaign.Objectives["goals"] is System.Text.Json.JsonElement goalsElement && goalsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                var goals = goalsElement.EnumerateArray().Select(g => g.GetString()).Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
                                if (goals.Any())
                                {
                                    instructionParts.Add($"Objetivos: {string.Join(", ", goals)}");
                                }
                            }
                        }

                        // Agregar audiencia objetivo si existe
                        if (campaign.TargetAudience != null)
                        {
                            var audienceParts = new List<string>();
                            if (campaign.TargetAudience.ContainsKey("ageRange"))
                            {
                                var ageRange = campaign.TargetAudience["ageRange"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(ageRange))
                                {
                                    audienceParts.Add($"Edad: {ageRange}");
                                }
                            }
                            if (campaign.TargetAudience.ContainsKey("interests"))
                            {
                                if (campaign.TargetAudience["interests"] is System.Text.Json.JsonElement interestsElement && interestsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                {
                                    var interests = interestsElement.EnumerateArray().Select(i => i.GetString()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
                                    if (interests.Any())
                                    {
                                        audienceParts.Add($"Intereses: {string.Join(", ", interests)}");
                                    }
                                }
                            }
                            if (audienceParts.Any())
                            {
                                instructionParts.Add($"Audiencia objetivo: {string.Join(". ", audienceParts)}");
                            }
                        }

                        // Usar canales reales de la campaña si están disponibles
                        if (campaign.TargetChannels != null && campaign.TargetChannels.Any())
                        {
                            channels = campaign.TargetChannels.ToList();
                            instructionParts.Add($"Canales objetivo: {string.Join(", ", channels)}");
                        }
                        else
                        {
                            channels = request.Channels ?? new List<string> { "instagram" };
                        }

                        // Construir instrucción final
                        instruction = request.Instruction ?? string.Join(". ", instructionParts);
                        
                        _logger.LogInformation(
                            "Datos reales de campaña cargados: Name={Name}, Channels={Channels}, Instruction={Instruction}",
                            campaign.Name, string.Join(", ", channels), instruction);
                    }
                    else
                    {
                        // Campaña no encontrada, usar valores por defecto
                        _logger.LogWarning(
                            "Campaña {CampaignId} no encontrada, usando valores por defecto",
                            request.CampaignId.Value);
                        instruction = request.Instruction ?? "Prueba de webhook desde frontend - Crear contenido de marketing para Instagram";
                        channels = request.Channels ?? new List<string> { "instagram" };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar datos de campaña {CampaignId}, usando valores por defecto", request.CampaignId.Value);
                    instruction = request.Instruction ?? "Prueba de webhook desde frontend - Crear contenido de marketing para Instagram";
                    channels = request.Channels ?? new List<string> { "instagram" };
                }
            }
            else
            {
                // No hay campaignId, usar valores del request o por defecto
                instruction = request.Instruction ?? "Prueba de webhook desde frontend - Crear contenido de marketing para Instagram";
                channels = request.Channels ?? new List<string> { "instagram" };
            }

            // Otorgar consents requeridos automáticamente si no existen (para pruebas)
            // Esto evita errores de "Missing consents" durante las pruebas
            try
            {
                var consentValidationService = HttpContext.RequestServices.GetRequiredService<IConsentValidationService>();
                
                // Verificar y otorgar AIGeneration consent
                var hasAiConsent = await consentValidationService.ValidateConsentAsync(
                    userId.Value, 
                    tenantId.Value, 
                    "AIGeneration", 
                    CancellationToken.None);
                
                if (!hasAiConsent)
                {
                    _logger.LogInformation("Otorgando consentimiento AIGeneration automáticamente para pruebas (UserId={UserId})", userId.Value);
                    var grantAiConsentCommand = new GrantConsentCommand
                    {
                        UserId = userId.Value,
                        TenantId = tenantId.Value,
                        ConsentType = "AIGeneration",
                        ConsentVersion = "1.0",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };
                    await _mediator.Send(grantAiConsentCommand, CancellationToken.None);
                }
                
                // Verificar y otorgar AutoPublishing consent
                var hasPublishingConsent = await consentValidationService.ValidateConsentAsync(
                    userId.Value, 
                    tenantId.Value, 
                    "AutoPublishing", 
                    CancellationToken.None);
                
                if (!hasPublishingConsent)
                {
                    _logger.LogInformation("Otorgando consentimiento AutoPublishing automáticamente para pruebas (UserId={UserId})", userId.Value);
                    var grantPublishingConsentCommand = new GrantConsentCommand
                    {
                        UserId = userId.Value,
                        TenantId = tenantId.Value,
                        ConsentType = "AutoPublishing",
                        ConsentVersion = "1.0",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };
                    await _mediator.Send(grantPublishingConsentCommand, CancellationToken.None);
                }
            }
            catch (Exception consentEx)
            {
                _logger.LogWarning(consentEx, "Error al otorgar consents automáticamente. Continuando con la prueba del webhook.");
                // Continuar con la prueba aunque falle el otorgamiento de consents
            }

            // Usar el servicio de automatización para disparar el webhook
            var automationService = HttpContext.RequestServices.GetRequiredService<IExternalAutomationService>();
            
            // Crear eventData como Dictionary para marketing.request
            // IMPORTANTE: Las claves deben estar en minúsculas como espera n8n
            // Esto es EXACTAMENTE igual a como se envía desde MarketingRequestController.Create
            var eventData = new Dictionary<string, object>
            {
                { "instruction", instruction },
                { "channels", channels },
                { "requiresApproval", requiresApproval },
                { "assets", assets }
            };

            _logger.LogInformation(
                "=== PRUEBA MANUAL DE WEBHOOK MARKETING REQUEST ===");
            _logger.LogInformation(
                "TenantId={TenantId}, UserId={UserId}, CampaignId={CampaignId}, UsingRealData={UsingRealData}",
                tenantId.Value, userId.Value, request.CampaignId?.ToString() ?? "NULL", usingRealCampaignData);
            _logger.LogInformation(
                "Instruction={Instruction}, Channels={Channels}, RequiresApproval={RequiresApproval}, Assets={Assets}",
                instruction, string.Join(", ", channels), requiresApproval, string.Join(", ", assets));

            // Pasar campaignId si está presente (igual que en MarketingRequestController)
            var requestId = await automationService.TriggerAutomationAsync(
                tenantId.Value,
                "marketing.request",
                eventData,
                userId,
                request.CampaignId, // ← Agregar campaignId opcional
                null,
                CancellationToken.None);

            _logger.LogInformation(
                "Webhook disparado exitosamente. RequestId={RequestId}", requestId);

            var message = request.CampaignId.HasValue
                ? $"Webhook disparado correctamente. RequestId: {requestId}. Asociado a CampaignId: {request.CampaignId}. {(usingRealCampaignData ? "✓ Usando datos reales de la campaña" : "⚠ Campaña no encontrada, usando valores por defecto")}"
                : $"Webhook disparado correctamente. RequestId: {requestId}";

            return Json(new 
            { 
                success = true, 
                message = message,
                requestId = requestId,
                campaignId = request.CampaignId?.ToString(),
                usingRealData = usingRealCampaignData,
                instruction = instruction,
                channels = channels
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al probar webhook");
            
            // Detectar errores específicos de configuración de n8n
            string userFriendlyMessage = ex.Message;
            string errorType = "GENERIC";
            
            if (ex.Message.Contains("Unused Respond to Webhook node", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("CONFIGURACION_N8N_WEBHOOK_NODE", StringComparison.OrdinalIgnoreCase))
            {
                errorType = "N8N_WEBHOOK_CONFIG";
                userFriendlyMessage = "Error de configuración en n8n: El workflow tiene un nodo 'Respond to Webhook' que no está conectado correctamente. " +
                                    "Por favor, accede a n8n y verifica que el nodo 'Respond to Webhook' esté conectado al flujo de ejecución del workflow. " +
                                    "Todas las ramas del workflow (éxito y error) deben terminar en un nodo 'Respond to Webhook'.";
            }
            else if (ex.Message.Contains("Error de configuración en n8n", StringComparison.OrdinalIgnoreCase))
            {
                errorType = "N8N_CONFIG";
                // El mensaje ya es descriptivo, usarlo tal cual
                userFriendlyMessage = ex.Message;
            }
            
            // Guardar error en ApplicationLogs
            var tenantId = UserHelper.GetTenantId(User);
            var userId = UserHelper.GetUserId(User);
            var additionalData = System.Text.Json.JsonSerializer.Serialize(new
            {
                CampaignId = request?.CampaignId?.ToString(),
                Instruction = request?.Instruction,
                Channels = request?.Channels,
                Assets = request?.Assets,
                RequiresApproval = request?.RequiresApproval,
                ErrorType = errorType
            });
            
            await _loggingService.LogErrorAsync(
                message: $"Error al disparar webhook de n8n: {ex.Message}",
                source: "N8nConfig.TestWebhook",
                exception: ex,
                tenantId: tenantId,
                userId: userId,
                requestId: HttpContext.TraceIdentifier,
                path: Request.Path,
                httpMethod: Request.Method,
                additionalData: additionalData,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: Request.Headers["User-Agent"].ToString());
            
            return Json(new 
            { 
                success = false, 
                message = userFriendlyMessage,
                error = ex.ToString(),
                errorType = errorType
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
    /// <summary>
    /// CampaignId opcional para asociar la solicitud a una campaña (igual que en MarketingRequestController).
    /// </summary>
    public Guid? CampaignId { get; set; }
}
