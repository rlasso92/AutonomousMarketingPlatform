using System.Net;
using System.Text.Json;
using AutonomousMarketingPlatform.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AutonomousMarketingPlatform.Web.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var tenantId = context.Items["TenantId"]?.ToString() ?? "Unknown";

        _logger.LogError(
            exception,
            "Unhandled exception: RequestId={RequestId}, TenantId={TenantId}, Path={Path}, Method={Method}, QueryString={QueryString}",
            requestId, tenantId, context.Request.Path, context.Request.Method, context.Request.QueryString);
        
        // Log detallado del stack trace siempre (incluso en producción para debugging)
        _logger.LogError("Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
            exception.GetType().FullName, exception.Message, exception.StackTrace);
        
        if (exception.InnerException != null)
        {
            _logger.LogError("Inner Exception: {InnerExceptionType}, Message: {InnerMessage}",
                exception.InnerException.GetType().FullName, exception.InnerException.Message);
        }

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = "VALIDATION_ERROR";
                errorResponse.Message = argEx.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Code = "UNAUTHORIZED";
                errorResponse.Message = "No autorizado para realizar esta operación";
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Code = "NOT_FOUND";
                errorResponse.Message = "Recurso no encontrado";
                break;

            case TenantNotResolvedException tenantEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Code = "TENANT_NOT_RESOLVED";
                errorResponse.Message = tenantEx.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Code = "INTERNAL_ERROR";
                
                // En producción, loguear el mensaje completo pero mostrar mensaje genérico al usuario
                var exceptionMessage = exception.Message;
                var innerExceptionMessage = exception.InnerException?.Message;
                
                // Log crítico: siempre loguear el mensaje completo
                _logger.LogError(
                    "INTERNAL_ERROR | RequestId={RequestId} | Path={Path} | Method={Method} | ExceptionType={ExceptionType} | Message={Message} | InnerMessage={InnerMessage}",
                    requestId,
                    context.Request.Path,
                    context.Request.Method,
                    exception.GetType().FullName,
                    exceptionMessage,
                    innerExceptionMessage ?? "N/A");
                
                errorResponse.Message = _environment.IsDevelopment()
                    ? exceptionMessage
                    : "Ha ocurrido un error interno. Por favor, intente más tarde.";
                
                // Solo incluir detalles en desarrollo
                if (_environment.IsDevelopment())
                {
                    errorResponse.Details = new
                    {
                        ExceptionType = exception.GetType().Name,
                        StackTrace = exception.StackTrace,
                        InnerException = exception.InnerException != null ? new
                        {
                            Type = exception.InnerException.GetType().Name,
                            Message = exception.InnerException.Message
                        } : null
                    };
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(new { error = errorResponse }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object? Details { get; set; }
    }
}

