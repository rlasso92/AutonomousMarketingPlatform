using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Web.Attributes;
using AutonomousMarketingPlatform.Web.Helpers;
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
    private readonly ILogger<MarketingRequestController> _logger;

    public MarketingRequestController(
        IExternalAutomationService automationService,
        ILogger<MarketingRequestController> logger)
    {
        _automationService = automationService;
        _logger = logger;
    }

    /// <summary>
    /// Muestra el formulario para solicitar contenido de marketing.
    /// </summary>
    [HttpGet]
    public IActionResult Create(Guid? campaignId = null)
    {
        ViewBag.CampaignId = campaignId;
        return View(new MarketingRequestDto
        {
            CampaignId = campaignId,
            Channels = new List<string> { "instagram" },
            RequiresApproval = false
        });
    }

    /// <summary>
    /// Procesa la solicitud de contenido de marketing y dispara el webhook a n8n.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
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
            var tenantId = UserHelper.GetTenantId(User);

            if (!userId.HasValue || !tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Preparar datos para el webhook
            var eventData = new Dictionary<string, object>
            {
                { "instruction", model.Instruction.Trim() },
                { "channels", model.Channels ?? new List<string>() },
                { "requiresApproval", model.RequiresApproval },
                { "assets", assetsList }
            };

            // Disparar webhook a n8n automáticamente
            var requestId = await _automationService.TriggerAutomationAsync(
                tenantId.Value,
                "marketing.request",
                eventData,
                userId,
                model.CampaignId,
                null,
                CancellationToken.None);

            _logger.LogInformation(
                "Solicitud de marketing enviada a n8n: TenantId={TenantId}, UserId={UserId}, RequestId={RequestId}",
                tenantId.Value,
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

