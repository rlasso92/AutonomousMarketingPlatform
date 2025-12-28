using AutonomousMarketingPlatform.Application.Services;
using MediatR;

namespace AutonomousMarketingPlatform.Application.UseCases.Automations;

/// <summary>
/// Query para obtener el estado de una ejecución de automatización externa.
/// </summary>
public class GetAutomationExecutionStatusQuery : IRequest<AutomationExecutionStatus>
{
    public Guid TenantId { get; set; }
    public string RequestId { get; set; } = string.Empty;
}

/// <summary>
/// Handler para obtener estado de ejecución.
/// </summary>
public class GetAutomationExecutionStatusQueryHandler : IRequestHandler<GetAutomationExecutionStatusQuery, AutomationExecutionStatus>
{
    private readonly IExternalAutomationService _automationService;

    public GetAutomationExecutionStatusQueryHandler(IExternalAutomationService automationService)
    {
        _automationService = automationService;
    }

    public async Task<AutomationExecutionStatus> Handle(GetAutomationExecutionStatusQuery request, CancellationToken cancellationToken)
    {
        return await _automationService.GetExecutionStatusAsync(
            request.TenantId,
            request.RequestId,
            cancellationToken);
    }
}

