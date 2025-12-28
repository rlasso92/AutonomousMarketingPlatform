using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Infrastructure.Services.Publishing;

/// <summary>
/// Servicio de fondo que procesa la cola de trabajos de publicación.
/// Implementa IHostedService para ejecutarse como background service.
/// </summary>
public class PublishingJobProcessorService : BackgroundService, IPublishingJobService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PublishingJobProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _schedulerInterval = TimeSpan.FromMinutes(1);

    public PublishingJobProcessorService(
        IServiceProvider serviceProvider,
        ILogger<PublishingJobProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PublishingJobProcessorService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Procesar trabajos pendientes
                await ProcessPendingJobsAsync(stoppingToken);

                // Verificar trabajos programados
                await ProcessScheduledJobsAsync(stoppingToken);

                // Esperar antes del siguiente ciclo
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ciclo de procesamiento de PublishingJobProcessorService");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("PublishingJobProcessorService detenido");
    }

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pendingJobs = await dbContext.PublishingJobs
            .Where(j => j.Status == "Pending" && j.IsActive)
            .OrderBy(j => j.CreatedAt)
            .Take(10) // Procesar máximo 10 a la vez
            .ToListAsync(cancellationToken);

        foreach (var job in pendingJobs)
        {
            try
            {
                await ProcessJobAsync(job.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando job {JobId}", job.Id);
            }
        }
    }

    private async Task ProcessScheduledJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        var scheduledJobs = await dbContext.PublishingJobs
            .Where(j => j.Status == "Pending" &&
                       j.ScheduledDate.HasValue &&
                       j.ScheduledDate <= now &&
                       j.IsActive)
            .OrderBy(j => j.ScheduledDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var job in scheduledJobs)
        {
            try
            {
                await ProcessJobAsync(job.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando job programado {JobId}", job.Id);
            }
        }
    }

    public async Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adapters = scope.ServiceProvider.GetServices<IPublishingAdapter>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var job = await dbContext.PublishingJobs
            .FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Job {JobId} no encontrado", jobId);
            return;
        }

        // Evitar procesamiento concurrente
        if (job.Status == "Processing")
        {
            _logger.LogWarning("Job {JobId} ya está siendo procesado", jobId);
            return;
        }

        try
        {
            // Marcar como procesando
            job.Status = "Processing";
            job.ProcessedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Procesando job {JobId} para canal {Channel}", jobId, job.Channel);

            // Obtener adaptador correspondiente
            var adapter = adapters.FirstOrDefault(a => a.ChannelName == job.Channel);
            if (adapter == null)
            {
                // Si no hay adaptador, usar manual (Fase A)
                adapter = scope.ServiceProvider.GetRequiredService<ManualPublishingAdapter>();
            }

            // Verificar si puede publicar
            var canPublish = await adapter.CanPublishAsync(cancellationToken);
            if (!canPublish && !job.RequiresApproval)
            {
                throw new InvalidOperationException($"Adaptador para {job.Channel} no puede publicar");
            }

            // Parsear payload
            var payload = !string.IsNullOrEmpty(job.Payload)
                ? JsonSerializer.Deserialize<PublishingPayload>(job.Payload)
                : new PublishingPayload
                {
                    Copy = job.Content,
                    Hashtags = job.Hashtags,
                    MediaUrls = !string.IsNullOrEmpty(job.MediaUrl) ? new List<string> { job.MediaUrl } : new List<string>()
                };

            if (payload == null)
            {
                throw new InvalidOperationException("Payload inválido");
            }

            // Si requiere aprobación (Fase A), generar paquete
            if (job.RequiresApproval || adapter.ChannelName == "Manual")
            {
                var package = await adapter.GeneratePackageAsync(payload, cancellationToken);
                
                // Guardar paquete como JSON en DownloadUrl (temporal)
                var packageJson = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
                job.DownloadUrl = $"data:application/json;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(packageJson))}";
                job.Status = "RequiresApproval";
                
                _logger.LogInformation("Job {JobId} marcado como RequiresApproval", jobId);
            }
            else
            {
                // Intentar publicar (Fase B)
                var result = await adapter.PublishAsync(payload, job.ScheduledDate, cancellationToken);

                if (result.Success)
                {
                    job.Status = "Success";
                    job.PublishedDate = DateTime.UtcNow;
                    job.PublishedUrl = result.PublishedUrl;
                    job.ExternalPostId = result.ExternalPostId;
                    job.ErrorMessage = null;
                }
                else
                {
                    // Manejar errores y reintentos
                    if (result.IsTransientError && job.RetryCount < job.MaxRetries)
                    {
                        job.Status = "Pending";
                        job.RetryCount++;
                        job.ErrorMessage = result.ErrorMessage;
                        
                        // Backoff exponencial
                        var delayMinutes = Math.Pow(5, job.RetryCount);
                        job.ScheduledDate = DateTime.UtcNow.AddMinutes(delayMinutes);
                        
                        _logger.LogWarning("Job {JobId} falló (transitorio), reintentando en {Delay} minutos. Intento {Retry}/{MaxRetries}",
                            jobId, delayMinutes, job.RetryCount, job.MaxRetries);
                    }
                    else
                    {
                        job.Status = "Failed";
                        job.ErrorMessage = result.ErrorMessage;
                        _logger.LogError("Job {JobId} falló permanentemente: {Error}", jobId, result.ErrorMessage);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            // Auditoría
            await auditService.LogAsync(
                job.TenantId,
                "ProcessPublishingJob",
                "PublishingJob",
                job.Id,
                null,
                null,
                null,
                null,
                null,
                job.Status,
                job.ErrorMessage,
                null,
                null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando job {JobId}", jobId);
            
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.RetryCount++;

            if (job.RetryCount >= job.MaxRetries)
            {
                job.Status = "Failed";
            }
            else
            {
                job.Status = "Pending";
                var delayMinutes = Math.Pow(5, job.RetryCount);
                job.ScheduledDate = DateTime.UtcNow.AddMinutes(delayMinutes);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Guid>> GetPendingJobsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.PublishingJobs
            .Where(j => j.Status == "Pending" && j.IsActive)
            .Select(j => j.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetScheduledJobsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        return await dbContext.PublishingJobs
            .Where(j => j.Status == "Pending" &&
                       j.ScheduledDate.HasValue &&
                       j.ScheduledDate <= now &&
                       j.IsActive)
            .Select(j => j.Id)
            .ToListAsync(cancellationToken);
    }
}

