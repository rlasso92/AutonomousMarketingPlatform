using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de logging que persiste logs en la base de datos.
/// NOTA: No usa ILogger para evitar dependencias circulares con el DatabaseLoggerProvider.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public LoggingService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task LogAsync(
        string level,
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        Exception? exception = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var log = new ApplicationLog
            {
                Level = level,
                Message = message,
                Source = source,
                TenantId = tenantId,
                UserId = userId,
                RequestId = requestId,
                Path = path,
                HttpMethod = httpMethod,
                AdditionalData = additionalData,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            if (exception != null)
            {
                log.ExceptionType = exception.GetType().FullName;
                log.StackTrace = exception.StackTrace;
                log.InnerException = exception.InnerException?.Message;
            }

            context.ApplicationLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Si falla al guardar el log, escribir a consola para no perder el error
            // No usar ILogger aquí para evitar dependencias circulares
            Console.WriteLine($"[ERROR] Error al persistir log en base de datos. Level={level}, Source={source}, Message={message}");
            Console.WriteLine($"[ERROR] Exception: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public async Task LogErrorAsync(
        string message,
        string source,
        Exception? exception = null,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            "Error",
            message,
            source,
            tenantId,
            userId,
            exception,
            requestId,
            path,
            httpMethod,
            additionalData,
            ipAddress,
            userAgent);
    }

    public async Task LogWarningAsync(
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            "Warning",
            message,
            source,
            tenantId,
            userId,
            null,
            requestId,
            path,
            httpMethod,
            additionalData,
            ipAddress,
            userAgent);
    }

    public async Task LogInformationAsync(
        string message,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            "Information",
            message,
            source,
            tenantId,
            userId,
            null,
            requestId,
            path,
            httpMethod,
            additionalData,
            ipAddress,
            userAgent);
    }

    public async Task LogCriticalAsync(
        string message,
        string source,
        Exception? exception = null,
        Guid? tenantId = null,
        Guid? userId = null,
        string? requestId = null,
        string? path = null,
        string? httpMethod = null,
        string? additionalData = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        await LogAsync(
            "Critical",
            message,
            source,
            tenantId,
            userId,
            exception,
            requestId,
            path,
            httpMethod,
            additionalData,
            ipAddress,
            userAgent);
    }
}


