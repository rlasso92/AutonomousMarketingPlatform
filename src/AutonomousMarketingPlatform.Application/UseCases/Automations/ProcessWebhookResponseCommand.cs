using AutonomousMarketingPlatform.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Automations;

/// <summary>
/// Comando para procesar una respuesta recibida de n8n vía webhook.
/// </summary>
public class ProcessWebhookResponseCommand : IRequest<bool>
{
    public Guid TenantId { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public WebhookResponseData ResponseData { get; set; } = null!;
}

/// <summary>
/// Handler para procesar respuesta de webhook.
/// </summary>
public class ProcessWebhookResponseCommandHandler : IRequestHandler<ProcessWebhookResponseCommand, bool>
{
    private readonly IExternalAutomationService _automationService;
    private readonly ILogger<ProcessWebhookResponseCommandHandler> _logger;

    public ProcessWebhookResponseCommandHandler(
        IExternalAutomationService automationService,
        ILogger<ProcessWebhookResponseCommandHandler> logger)
    {
        _automationService = automationService;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessWebhookResponseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar datos recibidos
            if (string.IsNullOrEmpty(request.RequestId))
            {
                _logger.LogWarning("Webhook recibido sin RequestId");
                return false;
            }

            if (request.ResponseData.RequestId != request.RequestId)
            {
                _logger.LogWarning(
                    "RequestId no coincide: Expected={Expected}, Received={Received}",
                    request.RequestId, request.ResponseData.RequestId);
                return false;
            }

            // Procesar respuesta
            await _automationService.ProcessWebhookResponseAsync(
                request.TenantId,
                request.RequestId,
                request.ResponseData,
                cancellationToken);

            _logger.LogInformation(
                "Webhook procesado exitosamente: RequestId={RequestId}, Status={Status}",
                request.RequestId, request.ResponseData.Status);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar webhook: RequestId={RequestId}", request.RequestId);
            return false;
        }
    }
}

