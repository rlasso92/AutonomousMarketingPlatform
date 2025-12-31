using AutonomousMarketingPlatform.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AutonomousMarketingPlatform.Infrastructure.Logging;

/// <summary>
/// Proveedor de logging que persiste logs en la base de datos.
/// </summary>
public class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly ILoggingService _loggingService;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new();

    public DatabaseLoggerProvider(
        ILoggingService loggingService,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _loggingService = loggingService;
        _httpContextAccessor = httpContextAccessor;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new DatabaseLogger(name, _loggingService, _httpContextAccessor));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

/// <summary>
/// Logger que persiste logs en la base de datos.
/// </summary>
public class DatabaseLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ILoggingService _loggingService;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public DatabaseLogger(
        string categoryName,
        ILoggingService loggingService,
        IHttpContextAccessor? httpContextAccessor)
    {
        _categoryName = categoryName;
        _loggingService = loggingService;
        _httpContextAccessor = httpContextAccessor;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Solo persistir logs de nivel Warning o superior para no saturar la base de datos
        // También se pueden configurar niveles específicos por categoría
        return logLevel >= LogLevel.Warning;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        try
        {
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            // Obtener información del contexto HTTP si está disponible
            var httpContext = _httpContextAccessor?.HttpContext;
            var tenantId = httpContext?.Items["TenantId"] as Guid?;
            var userId = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userIdGuid = !string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var uid) ? uid : (Guid?)null;
            var requestId = httpContext?.TraceIdentifier;
            var path = httpContext?.Request.Path.Value;
            var httpMethod = httpContext?.Request.Method;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            // Convertir LogLevel a string
            var level = logLevel switch
            {
                LogLevel.Critical => "Critical",
                LogLevel.Error => "Error",
                LogLevel.Warning => "Warning",
                LogLevel.Information => "Information",
                LogLevel.Debug => "Debug",
                LogLevel.Trace => "Trace",
                _ => "Information"
            };

            // Persistir el log de forma asíncrona (fire and forget para no bloquear)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _loggingService.LogAsync(
                        level,
                        message,
                        _categoryName,
                        tenantId,
                        userIdGuid,
                        exception,
                        requestId,
                        path,
                        httpMethod,
                        null, // additionalData - se puede extender si es necesario
                        ipAddress,
                        userAgent);
                }
                catch
                {
                    // Silenciar errores al persistir logs para no causar loops
                }
            });
        }
        catch
        {
            // Silenciar errores para no causar loops de logging
        }
    }
}

