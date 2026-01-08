using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousMarketingPlatform.Web.Controllers;

/// <summary>
/// Controlador para solicitar creación de contenido de marketing mediante n8n.
/// Requiere rol Marketer, Admin o Owner.
/// </summary>
[Authorize]
[AuthorizeRole("Marketer", "Admin", "Owner")]
public class MarketingRequestController : Controller
{
    private readonly IExternalAutomationService _automationService;
    private readonly IMediator _mediator;
    private readonly ILogger<MarketingRequestController> _logger;

    public MarketingRequestController(
        IExternalAutomationService automationService,
        IMediator mediator,
        ILogger<MarketingRequestController> logger)
    {
        _automationService = automationService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Muestra el formulario para solicitar contenido de marketing.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(Guid? campaignId = null)
    {
        try
        {
            _logger.LogInformation("=== INICIO MarketingRequestController.Create (GET) ===");
            _logger.LogInformation("Request Path: {Path}", HttpContext.Request.Path);
            _logger.LogInformation("Query String: {QueryString}", HttpContext.Request.QueryString);
            _logger.LogInformation("CampaignId recibido: {CampaignId}", campaignId?.ToString() ?? "NULL");
            
            // Verificar autenticación
            _logger.LogInformation("User autenticado: {IsAuthenticated}", User?.Identity?.IsAuthenticated ?? false);
            _logger.LogInformation("User Name: {UserName}", User?.Identity?.Name ?? "NULL");
            
            // Verificar roles
            var roles = User?.Claims?.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Select(c => c.Value)?.ToList() ?? new List<string>();
            _logger.LogInformation("Roles del usuario: {Roles}", string.Join(", ", roles));
            
            // Verificar claims específicos
            var isSuperAdmin = User?.HasClaim("IsSuperAdmin", "true") ?? false;
            _logger.LogInformation("IsSuperAdmin: {IsSuperAdmin}", isSuperAdmin);
            
            var userId = UserHelper.GetUserId(User);
            var tenantId = UserHelper.GetTenantId(User);
            _logger.LogInformation("UserId: {UserId}, TenantId: {TenantId}", 
                userId?.ToString() ?? "NULL", 
                tenantId?.ToString() ?? "NULL");
            
            ViewBag.CampaignId = campaignId;
            
            // Inicializar canales por defecto
            var defaultChannels = new List<string> { "instagram" };
            var channels = defaultChannels;
            
            // Si hay un campaignId, cargar los canales de la campaña
            if (campaignId.HasValue)
            {
                try
                {
                    var campaignRepository = HttpContext.RequestServices
                        .GetRequiredService<Domain.Interfaces.ICampaignRepository>();
                    
                    // Determinar el tenantId efectivo (para SuperAdmin usar Guid.Empty)
                    var effectiveTenantId = isSuperAdmin ? Guid.Empty : (tenantId ?? Guid.Empty);
                    
                    var campaign = await campaignRepository.GetCampaignWithDetailsAsync(
                        campaignId.Value,
                        effectiveTenantId,
                        CancellationToken.None);
                    
                    if (campaign != null && !string.IsNullOrWhiteSpace(campaign.TargetChannels))
                    {
                        try
                        {
                            channels = System.Text.Json.JsonSerializer.Deserialize<List<string>>(campaign.TargetChannels) 
                                ?? defaultChannels;
                            
                            // Normalizar canales a lowercase para consistencia
                            channels = channels.Select(c => c.ToLowerInvariant().Trim())
                                .Where(c => !string.IsNullOrWhiteSpace(c))
                                .ToList();
                            
                            _logger.LogInformation(
                                "Canales cargados de la campaña {CampaignId}: {Channels}",
                                campaignId.Value,
                                string.Join(", ", channels));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, 
                                "Error al deserializar TargetChannels de la campaña {CampaignId}, usando canales por defecto",
                                campaignId.Value);
                            channels = defaultChannels;
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Campaña {CampaignId} no encontrada o sin TargetChannels, usando canales por defecto",
                            campaignId.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error al cargar campaña {CampaignId}, usando canales por defecto",
                        campaignId.Value);
                    channels = defaultChannels;
                }
            }
            
            var model = new MarketingRequestDto
            {
                CampaignId = campaignId,
                Channels = channels,
                RequiresApproval = false
            };
            
            _logger.LogInformation("Model creado: CampaignId={CampaignId}, Channels={Channels}", 
                model.CampaignId?.ToString() ?? "NULL",
                string.Join(", ", model.Channels ?? new List<string>()));
            
            _logger.LogInformation("=== FIN MarketingRequestController.Create (GET) - Retornando View ===");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== ERROR en MarketingRequestController.Create (GET) ===");
            _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
            _logger.LogError("Exception Message: {Message}", ex.Message);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerException}", ex.InnerException.Message);
            }
            
            // Retornar error 500 en lugar de 400 para que se vea el error real
            throw;
        }
    }

    /// <summary>
    /// Procesa la solicitud de contenido de marketing y dispara el webhook a n8n.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(MarketingRequestDto model)
    {
        // Procesar canales desde JSON
        if (!string.IsNullOrWhiteSpace(model.ChannelsJson))
        {
            try
            {
                model.Channels = System.Text.Json.JsonSerializer.Deserialize<List<string>>(model.ChannelsJson);
            }
            catch
            {
                model.Channels = new List<string>();
            }
        }

        // Procesar assets desde texto (una URL por línea)
        var assetsList = new List<string>();
        if (!string.IsNullOrWhiteSpace(model.AssetsText))
        {
            assetsList = model.AssetsText
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(url => url.Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out _))
                .ToList();
        }

        // Validar que al menos un canal esté seleccionado
        if (model.Channels == null || model.Channels.Count == 0)
        {
            ModelState.AddModelError("Channels", "Debes seleccionar al menos un canal de publicación.");
        }

        // Validar instrucción
        if (string.IsNullOrWhiteSpace(model.Instruction))
        {
            ModelState.AddModelError("Instruction", "La instrucción es requerida.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CampaignId = model.CampaignId;
            model.Assets = assetsList;
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

            // Determinar el tenantId a usar para la configuración de n8n
            Guid effectiveTenantId;
            
            // Si es SuperAdmin y hay un CampaignId, obtener el TenantId de la campaña
            if (isSuperAdmin && model.CampaignId.HasValue)
            {
                try
                {
                    var campaignRepository = HttpContext.RequestServices
                    .GetRequiredService<Domain.Interfaces.ICampaignRepository>();
                    
                    // Obtener la campaña directamente (SuperAdmin puede ver cualquier campaña)
                    var campaignEntity = await campaignRepository.GetCampaignWithDetailsAsync(
                        model.CampaignId.Value, 
                        Guid.Empty, // No filtrar por tenant para SuperAdmin
                        CancellationToken.None);
                    
                    if (campaignEntity != null)
                    {
                        effectiveTenantId = campaignEntity.TenantId;
                        _logger.LogInformation(
                            "SuperAdmin usando TenantId de la campaña: {CampaignId} -> {TenantId}",
                            model.CampaignId.Value, effectiveTenantId);
                    }
                    else
                    {
                        effectiveTenantId = currentTenantId ?? Guid.Empty;
                        _logger.LogWarning(
                            "Campaña {CampaignId} no encontrada, usando TenantId del usuario: {TenantId}",
                            model.CampaignId.Value, effectiveTenantId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener TenantId de la campaña {CampaignId}", model.CampaignId.Value);
                    effectiveTenantId = currentTenantId ?? Guid.Empty;
                }
            }
            else if (currentTenantId.HasValue)
            {
                effectiveTenantId = currentTenantId.Value;
            }
            else
            {
                _logger.LogError("No se pudo determinar TenantId para la solicitud");
                ModelState.AddModelError(string.Empty, "No se pudo determinar el tenant. Por favor, contacta al administrador.");
                ViewBag.CampaignId = model.CampaignId;
                return View(model);
            }

            // Preparar datos para el webhook
            var eventData = new Dictionary<string, object>
            {
                { "instruction", model.Instruction.Trim() },
                { "channels", model.Channels ?? new List<string>() },
                { "requiresApproval", model.RequiresApproval },
                { "assets", assetsList }
            };

            _logger.LogInformation(
                "Enviando solicitud de marketing: EffectiveTenantId={EffectiveTenantId}, CampaignId={CampaignId}, UserId={UserId}",
                effectiveTenantId, model.CampaignId?.ToString() ?? "NULL", userId.Value);

            // Disparar webhook a n8n automáticamente usando el TenantId efectivo
            var requestId = await _automationService.TriggerAutomationAsync(
                effectiveTenantId,
                "marketing.request",
                eventData,
                userId,
                model.CampaignId,
                null,
                CancellationToken.None);

            _logger.LogInformation(
                "Solicitud de marketing enviada a n8n: TenantId={TenantId}, UserId={UserId}, RequestId={RequestId}",
                effectiveTenantId,
                userId.Value,
                requestId);

            TempData["SuccessMessage"] = 
                $"¡Solicitud de contenido de marketing enviada exitosamente! " +
                $"Request ID: {requestId}. El contenido se generará automáticamente mediante n8n.";

            return RedirectToAction(nameof(Success), new { requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar solicitud de marketing a n8n");
            ModelState.AddModelError(string.Empty, 
                $"Error al enviar la solicitud: {ex.Message}. Por favor, intente nuevamente.");
            ViewBag.CampaignId = model.CampaignId;
            return View(model);
        }
    }

    /// <summary>
    /// Muestra la página de éxito después de enviar la solicitud.
    /// </summary>
    [HttpGet]
    public IActionResult Success(string? requestId = null)
    {
        ViewBag.RequestId = requestId;
        return View();
    }
}

    /// <summary>
    /// DTO para solicitar contenido de marketing.
    /// </summary>
public class MarketingRequestDto
{
    /// <summary>
    /// ID de la campaña asociada (opcional).
    /// </summary>
    public Guid? CampaignId { get; set; }

    /// <summary>
    /// Instrucción o descripción del contenido a generar.
    /// </summary>
    public string Instruction { get; set; } = string.Empty;

    /// <summary>
    /// Canales donde se publicará el contenido (JSON string desde el formulario).
    /// </summary>
    public string? ChannelsJson { get; set; }

    /// <summary>
    /// Canales procesados.
    /// </summary>
    public List<string>? Channels { get; set; }

    /// <summary>
    /// Indica si requiere aprobación manual antes de publicar.
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// URLs de assets como texto (una por línea).
    /// </summary>
    public string? AssetsText { get; set; }

    /// <summary>
    /// URLs de assets procesadas.
    /// </summary>
    public List<string>? Assets { get; set; }
}

