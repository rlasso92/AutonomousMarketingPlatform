using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Metrics;

/// <summary>
/// Comando para registrar métricas de una campaña.
/// </summary>
public class RegisterCampaignMetricsCommand : IRequest<CampaignMetricsDto>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public RegisterCampaignMetricsDto Metrics { get; set; } = null!;
}

/// <summary>
/// Handler para registrar métricas de campaña.
/// </summary>
public class RegisterCampaignMetricsCommandHandler : IRequestHandler<RegisterCampaignMetricsCommand, CampaignMetricsDto>
{
    private readonly IMetricsService _metricsService;
    private readonly ISecurityService _securityService;
    private readonly IMemoryLearningService _learningService;
    private readonly IValidator<RegisterCampaignMetricsDto> _validator;
    private readonly ILogger<RegisterCampaignMetricsCommandHandler> _logger;

    public RegisterCampaignMetricsCommandHandler(
        IMetricsService metricsService,
        ISecurityService securityService,
        IMemoryLearningService learningService,
        IValidator<RegisterCampaignMetricsDto> validator,
        ILogger<RegisterCampaignMetricsCommandHandler> logger)
    {
        _metricsService = metricsService;
        _securityService = securityService;
        _learningService = learningService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<CampaignMetricsDto> Handle(RegisterCampaignMetricsCommand request, CancellationToken cancellationToken)
    {
        // Validar DTO
        var validationResult = await _validator.ValidateAsync(request.Metrics, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Validar usuario pertenece al tenant
        var userBelongsToTenant = await _securityService.ValidateUserBelongsToTenantAsync(
            request.UserId, request.TenantId, cancellationToken);
        
        if (!userBelongsToTenant)
        {
            throw new UnauthorizedAccessException("Usuario no pertenece a este tenant");
        }

        var result = await _metricsService.RegisterCampaignMetricsAsync(
            request.TenantId,
            request.Metrics,
            cancellationToken);

        _logger.LogInformation("Métricas de campaña registradas: CampaignId={CampaignId}, Date={Date}",
            request.Metrics.CampaignId, request.Metrics.MetricDate);

        // Trigger automático de aprendizaje (en background para no bloquear la respuesta)
        _ = Task.Run(async () =>
        {
            try
            {
                await _learningService.LearnFromCampaignMetricsAsync(
                    request.TenantId,
                    request.Metrics.CampaignId,
                    cancellationToken);
                _logger.LogInformation("Aprendizaje automático completado para campaña {CampaignId}", 
                    request.Metrics.CampaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en aprendizaje automático para campaña {CampaignId}", 
                    request.Metrics.CampaignId);
            }
        }, cancellationToken);

        return result;
    }
}

