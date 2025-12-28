using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Interfaces;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Background service que procesa aprendizaje automático desde métricas periódicamente.
/// Se ejecuta diariamente para analizar métricas recientes y actualizar la memoria.
/// </summary>
public class MetricsLearningBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsLearningBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromDays(1); // Ejecutar diariamente

    public MetricsLearningBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MetricsLearningBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MetricsLearningBackgroundService iniciado. Se ejecutará diariamente.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessLearningAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proceso de aprendizaje automático desde métricas");
            }

            // Esperar hasta la próxima ejecución (24 horas)
            await Task.Delay(_period, stoppingToken);
        }
    }

    private async Task ProcessLearningAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando proceso de aprendizaje desde métricas...");

        using var scope = _serviceProvider.CreateScope();
        var learningService = scope.ServiceProvider.GetRequiredService<IMemoryLearningService>();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<Infrastructure.Data.ApplicationDbContext>>();

        // Obtener todos los tenants activos desde la base de datos
        using var dbContext = dbContextFactory.CreateDbContext();
        var tenants = await dbContext.Tenants
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        if (!tenants.Any())
        {
            _logger.LogWarning("No se encontraron tenants activos para procesar aprendizaje");
            return;
        }

        _logger.LogInformation("Procesando aprendizaje para {Count} tenants", tenants.Count);

        foreach (var tenantId in tenants)
        {
            try
            {
                _logger.LogInformation("Procesando aprendizaje para tenant {TenantId}", tenantId);
                await learningService.ProcessLearningFromRecentMetricsAsync(
                    tenantId,
                    daysToAnalyze: 7,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar aprendizaje para tenant {TenantId}", tenantId);
                // Continuar con el siguiente tenant aunque falle uno
            }
        }

        _logger.LogInformation("Proceso de aprendizaje completado para todos los tenants");
    }
}

