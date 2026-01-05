using AutonomousMarketingPlatform.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;

namespace AutonomousMarketingPlatform.Application.UseCases.N8n;

/// <summary>
/// Command para probar la conexión con n8n.
/// </summary>
public class TestN8nConnectionCommand : IRequest<TestN8nConnectionResponse>
{
    public TestN8nConnectionDto Request { get; set; } = null!;
}

/// <summary>
/// Handler para probar la conexión con n8n.
/// </summary>
public class TestN8nConnectionCommandHandler : IRequestHandler<TestN8nConnectionCommand, TestN8nConnectionResponse>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestN8nConnectionCommandHandler> _logger;

    public TestN8nConnectionCommandHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<TestN8nConnectionCommandHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<TestN8nConnectionResponse> Handle(TestN8nConnectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Request == null)
            {
                _logger.LogWarning("TestN8nConnectionCommand recibido con Request null");
                return new TestN8nConnectionResponse
                {
                    Success = false,
                    Message = "Request no puede ser null",
                    Error = "Request is required"
                };
            }

            if (string.IsNullOrWhiteSpace(request.Request.BaseUrl))
            {
                _logger.LogWarning("TestN8nConnectionCommand recibido con BaseUrl vacío");
                return new TestN8nConnectionResponse
                {
                    Success = false,
                    Message = "URL base no puede estar vacía",
                    Error = "BaseUrl is required"
                };
            }

            // Crear HttpClient con configuración apropiada
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10); // Aumentado de 5 a 10 segundos
            
            // Agregar headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "AutonomousMarketingPlatform/1.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Intentar conectar a la URL base de n8n
            // n8n puede tener diferentes endpoints de health, probar varios
            var baseUrl = request.Request.BaseUrl.TrimEnd('/');
            var testUrls = new[]
            {
                $"{baseUrl}/healthz",  // Endpoint estándar de n8n
                $"{baseUrl}/health",   // Alternativa
                $"{baseUrl}/",          // Root (siempre responde)
                $"{baseUrl}/webhook"    // Webhook base (siempre responde)
            };

            _logger.LogInformation("Probando conexión con n8n. BaseUrl: {BaseUrl}", baseUrl);

            HttpResponseMessage? response = null;
            string? successfulUrl = null;
            Exception? lastException = null;

            // Probar cada URL hasta encontrar una que responda
            foreach (var testUrl in testUrls)
            {
                try
                {
                    _logger.LogInformation("Intentando conectar a: {Url}", testUrl);
                    response = await httpClient.GetAsync(testUrl, cancellationToken);
                    
                    // Si obtenemos cualquier respuesta (incluso 404), significa que n8n está accesible
                    if (response != null)
                    {
                        successfulUrl = testUrl;
                        _logger.LogInformation("Conexión exitosa a: {Url}, StatusCode: {StatusCode}", testUrl, response.StatusCode);
                        break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Error al conectar con n8n en: {Url}", testUrl);
                    lastException = ex;
                    continue; // Intentar siguiente URL
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Timeout al conectar con n8n en: {Url}", testUrl);
                    lastException = ex;
                    continue; // Intentar siguiente URL
                }
            }

            // Si ninguna URL funcionó, retornar error
            if (response == null)
            {
                var errorMessage = lastException?.Message ?? "No se pudo conectar con ninguna URL de n8n";
                _logger.LogError("No se pudo conectar con n8n después de probar {Count} URLs. Último error: {Error}", 
                    testUrls.Length, errorMessage);
                return new TestN8nConnectionResponse
                {
                    Success = false,
                    Message = $"No se pudo conectar con n8n: {errorMessage}. Verifica que n8n esté corriendo y accesible desde Render.",
                    Error = errorMessage,
                    StatusCode = null
                };
            }

            if (response != null)
            {
                var statusCode = (int)response.StatusCode;
                var isSuccess = response.IsSuccessStatusCode;

                if (isSuccess)
                {
                    _logger.LogInformation("Conexión exitosa con n8n: StatusCode={StatusCode}", statusCode);
                    return new TestN8nConnectionResponse
                    {
                        Success = true,
                        Message = $"Conexión exitosa con n8n (Status: {statusCode})",
                        StatusCode = statusCode
                    };
                }
                else
                {
                    _logger.LogWarning("n8n respondió con error: StatusCode={StatusCode}", statusCode);
                    return new TestN8nConnectionResponse
                    {
                        Success = false,
                        Message = $"n8n respondió con error (Status: {statusCode})",
                        StatusCode = statusCode
                    };
                }
            }

            return new TestN8nConnectionResponse
            {
                Success = false,
                Message = "No se recibió respuesta de n8n",
                Error = "No response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al probar conexión con n8n");
            return new TestN8nConnectionResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Error = ex.Message
            };
        }
    }
}

