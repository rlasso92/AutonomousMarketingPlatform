using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Automations;

/// <summary>
/// Comando para disparar una automatización externa cuando ocurre un evento.
/// </summary>
public class TriggerExternalAutomationCommand : IRequest<string> // Retorna RequestId
{
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public AutomationEventDto EventData { get; set; } = null!;
    public Guid? UserId { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public Dictionary<string, object>? AdditionalContext { get; set; }
}

/// <summary>
/// Handler para disparar automatización externa.
/// </summary>
public class TriggerExternalAutomationCommandHandler : IRequestHandler<TriggerExternalAutomationCommand, string>
{
    private readonly IExternalAutomationService _automationService;
    private readonly IMarketingMemoryService _memoryService;
    private readonly ILogger<TriggerExternalAutomationCommandHandler> _logger;

    public TriggerExternalAutomationCommandHandler(
        IExternalAutomationService automationService,
        IMarketingMemoryService memoryService,
        ILogger<TriggerExternalAutomationCommandHandler> logger)
    {
        _automationService = automationService;
        _memoryService = memoryService;
        _logger = logger;
    }

    public async Task<string> Handle(TriggerExternalAutomationCommand request, CancellationToken cancellationToken)
    {
        // Obtener contexto de memoria si es necesario
        Dictionary<string, object>? memoryContext = null;
        if (request.UserId.HasValue)
        {
            try
            {
                var memoryContextForAI = await _memoryService.GetMemoryContextForAIAsync(
                    request.TenantId,
                    request.UserId,
                    null,
                    null,
                    cancellationToken);

                memoryContext = new Dictionary<string, object>
                {
                    { "UserPreferences", memoryContextForAI.UserPreferences },
                    { "RecentConversations", memoryContextForAI.RecentConversations },
                    { "Learnings", memoryContextForAI.Learnings }
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener contexto de memoria para automatización");
            }
        }

        // Construir datos adicionales
        var additionalContext = request.AdditionalContext ?? new Dictionary<string, object>();
        if (memoryContext != null)
        {
            additionalContext["MemoryContext"] = memoryContext;
        }

        // Disparar automatización
        var requestId = await _automationService.TriggerAutomationAsync(
            request.TenantId,
            request.EventType,
            request.EventData,
            request.UserId,
            request.RelatedEntityId,
            additionalContext,
            cancellationToken);

        _logger.LogInformation(
            "Automatización externa disparada: EventType={EventType}, RequestId={RequestId}, TenantId={TenantId}",
            request.EventType, requestId, request.TenantId);

        return requestId;
    }
}

