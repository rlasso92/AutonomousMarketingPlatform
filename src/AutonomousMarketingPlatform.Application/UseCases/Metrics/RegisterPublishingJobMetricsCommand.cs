using AutonomousMarketingPlatform.Application.DTOs;
using AutonomousMarketingPlatform.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Application.UseCases.Metrics;

/// <summary>
/// Comando para registrar métricas de una publicación.
/// </summary>
public class RegisterPublishingJobMetricsCommand : IRequest<PublishingJobMetricsDto>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public RegisterPublishingJobMetricsDto Metrics { get; set; } = null!;
}

/// <summary>
/// Handler para registrar métricas de publicación.
/// </summary>
public class RegisterPublishingJobMetricsCommandHandler : IRequestHandler<RegisterPublishingJobMetricsCommand, PublishingJobMetricsDto>
{
    private readonly IMetricsService _metricsService;
    private readonly ISecurityService _securityService;
    private readonly IMemoryLearningService _learningService;
    private readonly IValidator<RegisterPublishingJobMetricsDto> _validator;
    private readonly ILogger<RegisterPublishingJobMetricsCommandHandler> _logger;

    public RegisterPublishingJobMetricsCommandHandler(
        IMetricsService metricsService,
        ISecurityService securityService,
        IMemoryLearningService learningService,
        IValidator<RegisterPublishingJobMetricsDto> validator,
        ILogger<RegisterPublishingJobMetricsCommandHandler> logger)
    {
        _metricsService = metricsService;
        _securityService = securityService;
        _learningService = learningService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PublishingJobMetricsDto> Handle(RegisterPublishingJobMetricsCommand request, CancellationToken cancellationToken)
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

        var result = await _metricsService.RegisterPublishingJobMetricsAsync(
            request.TenantId,
            request.Metrics,
            cancellationToken);

        _logger.LogInformation("Métricas de publicación registradas: PublishingJobId={PublishingJobId}, Date={Date}",
            request.Metrics.PublishingJobId, request.Metrics.MetricDate);

        // Trigger automático de aprendizaje (en background para no bloquear la respuesta)
        _ = Task.Run(async () =>
        {
            try
            {
                await _learningService.LearnFromPublishingJobMetricsAsync(
                    request.TenantId,
                    request.Metrics.PublishingJobId,
                    cancellationToken);
                _logger.LogInformation("Aprendizaje automático completado para publicación {PublishingJobId}", 
                    request.Metrics.PublishingJobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en aprendizaje automático para publicación {PublishingJobId}", 
                    request.Metrics.PublishingJobId);
            }
        }, cancellationToken);

        return result;
    }
}

