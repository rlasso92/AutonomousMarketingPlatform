using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AutonomousMarketingPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de automatizaciones externas que se conecta con n8n.
/// </summary>
public class ExternalAutomationService : IExternalAutomationService
{
    private readonly IConfiguration _configuration;
    private readonly IRepository<TenantN8nConfig> _configRepository;
    private readonly ILogger<ExternalAutomationService> _logger;
    private readonly HttpClient _httpClient;

    public ExternalAutomationService(
        IConfiguration configuration,
        IRepository<TenantN8nConfig> configRepository,
        ILogger<ExternalAutomationService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _configRepository = configRepository;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> TriggerAutomationAsync(
        Guid tenantId,
        string eventType,
        object eventData,
        Guid? userId = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Triggering external automation: TenantId={TenantId}, EventType={EventType}",
            tenantId,
            eventType);

        // Obtener configuración de n8n desde la base de datos
        var n8nConfig = await GetN8nConfigAsync(tenantId, cancellationToken);

        // Si está en modo mock, simular
        if (n8nConfig.UseMock)
        {
            await Task.Delay(100, cancellationToken);
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Mock mode: Returning requestId={RequestId}", requestId);
            return requestId;
        }

        try
        {
            // Obtener URL del webhook según el tipo de evento
            var webhookUrl = GetWebhookUrlForEventType(eventType, n8nConfig);
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogWarning(
                    "No webhook URL configured for event type: {EventType}. Using mock.",
                    eventType);
                return Guid.NewGuid().ToString();
            }
            
            _logger.LogInformation(
                "Using webhook URL for event type {EventType}: {WebhookUrl}",
                eventType,
                webhookUrl);

            // Construir payload para n8n según el tipo de evento
            object payload;
            
            if (eventType == "marketing.request" && eventData != null)
            {
                // Para marketing.request, extraer los campos del eventData y enviarlos directamente
                // n8n espera: tenantId, userId, instruction, channels, requiresApproval, campaignId, assets
                var eventDataDict = eventData as Dictionary<string, object>;
                if (eventDataDict != null)
                {
                    // n8n espera: tenantId, userId, instruction, channels, requiresApproval (todos en minúsculas)
                    // Validar que userId no sea null o vacío (n8n requiere notEmpty)
                    if (!userId.HasValue)
                    {
                        throw new ArgumentException("userId is required for marketing.request event type");
                    }
                    
                    payload = new Dictionary<string, object>
                    {
                        { "tenantId", tenantId.ToString() },
                        { "userId", userId.Value.ToString() },
                        { "instruction", eventDataDict.GetValueOrDefault("instruction")?.ToString() ?? 
                                       eventDataDict.GetValueOrDefault("Instruction")?.ToString() ?? "" },
                        { "channels", eventDataDict.GetValueOrDefault("channels") ?? 
                                    eventDataDict.GetValueOrDefault("Channels") ?? new List<string>() },
                        { "requiresApproval", eventDataDict.GetValueOrDefault("requiresApproval") ?? 
                                             eventDataDict.GetValueOrDefault("RequiresApproval") ?? false },
                        { "campaignId", relatedEntityId?.ToString() ?? null },
                        { "assets", eventDataDict.GetValueOrDefault("assets") ?? 
                                  eventDataDict.GetValueOrDefault("Assets") ?? new List<string>() }
                    };
                }
                else
                {
                    // Si eventData no es un Dictionary, intentar serializarlo y deserializarlo
                    if (!userId.HasValue)
                    {
                        throw new ArgumentException("userId is required for marketing.request event type");
                    }
                    
                    var jsonString = JsonSerializer.Serialize(eventData);
                    var eventDataObj = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                    
                    payload = new Dictionary<string, object>
                    {
                        { "tenantId", tenantId.ToString() },
                        { "userId", userId.Value.ToString() },
                        { "instruction", eventDataObj?.GetValueOrDefault("instruction")?.ToString() ?? 
                                         eventDataObj?.GetValueOrDefault("Instruction")?.ToString() ?? "" },
                        { "channels", eventDataObj?.GetValueOrDefault("channels") ?? 
                                    eventDataObj?.GetValueOrDefault("Channels") ?? new List<string>() },
                        { "requiresApproval", eventDataObj?.GetValueOrDefault("requiresApproval") ?? 
                                             eventDataObj?.GetValueOrDefault("RequiresApproval") ?? false },
                        { "campaignId", relatedEntityId?.ToString() ?? null },
                        { "assets", eventDataObj?.GetValueOrDefault("assets") ?? 
                                  eventDataObj?.GetValueOrDefault("Assets") ?? new List<string>() }
                    };
                }
            }
            else
            {
                // Para otros tipos de eventos, usar el formato estándar
                payload = new
                {
                    tenantId = tenantId.ToString(),
                    userId = userId?.ToString(),
                    eventType = eventType,
                    eventData = eventData,
                    relatedEntityId = relatedEntityId?.ToString(),
                    additionalContext = additionalContext ?? new Dictionary<string, object>(),
                    timestamp = DateTime.UtcNow
                };
            }

            // Llamar al webhook de n8n
            HttpResponseMessage response;
            if (payload is Dictionary<string, object> dictPayload)
            {
                // Serializar manualmente sin cambiar las claves (n8n espera exactamente estos nombres)
                var jsonContent = JsonSerializer.Serialize(dictPayload, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                _logger.LogInformation(
                    "Enviando POST a n8n webhook: URL={WebhookUrl}, Payload={Payload}",
                    webhookUrl,
                    jsonContent);
                
                response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
                
                _logger.LogInformation(
                    "Respuesta de n8n: StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}",
                    response.StatusCode,
                    response.ReasonPhrase);
            }
            else
            {
                _logger.LogInformation("Enviando POST a n8n webhook: URL={WebhookUrl}", webhookUrl);
                response = await _httpClient.PostAsJsonAsync(
                    webhookUrl,
                    payload,
                    cancellationToken);
                _logger.LogInformation(
                    "Respuesta de n8n: StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}",
                    response.StatusCode,
                    response.ReasonPhrase);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Error al llamar a n8n webhook: StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Response={Response}, URL={WebhookUrl}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    errorContent,
                    webhookUrl);
                throw new HttpRequestException(
                    $"Error al llamar a n8n: {response.StatusCode} {response.ReasonPhrase}. URL: {webhookUrl}. Response: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Manejo defensivo: n8n puede responder vacío, texto plano, HTML o JSON
            string requestId;
            
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning(
                    "n8n webhook respondió vacío para evento {EventType}. Generando requestId automático.",
                    eventType);
                requestId = Guid.NewGuid().ToString();
            }
            else
            {
                // Intentar deserializar como JSON
                Dictionary<string, object>? responseData = null;
                try
                {
                    responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        responseContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "n8n webhook respondió con contenido no-JSON para evento {EventType}. Response: {Response}",
                        eventType,
                        responseContent);
                    // Continuar con requestId generado
                }
                
                // Extraer requestId si está disponible en la respuesta JSON
                if (responseData != null && responseData.ContainsKey("requestId"))
                {
                    requestId = responseData["requestId"]?.ToString() ?? Guid.NewGuid().ToString();
                }
                else if (responseData != null && responseData.ContainsKey("executionId"))
                {
                    // Algunos workflows de n8n devuelven executionId en lugar de requestId
                    requestId = responseData["executionId"]?.ToString() ?? Guid.NewGuid().ToString();
                }
                else
                {
                    // Generar un requestId si no está en la respuesta
                    requestId = Guid.NewGuid().ToString();
                    _logger.LogInformation(
                        "n8n no devolvió requestId en la respuesta. Generado: {RequestId}",
                        requestId);
                }
            }

            _logger.LogInformation(
                "n8n workflow triggered successfully: EventType={EventType}, RequestId={RequestId}, ResponseLength={ResponseLength}",
                eventType,
                requestId,
                responseContent?.Length ?? 0);

            return requestId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Error calling n8n webhook for event type: {EventType}",
                eventType);
            throw new InvalidOperationException($"Error al llamar a n8n: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error triggering automation: EventType={EventType}",
                eventType);
            throw;
        }
    }

    public async Task<AutomationExecutionStatus> GetExecutionStatusAsync(
        Guid tenantId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting execution status: TenantId={TenantId}, RequestId={RequestId}",
            tenantId,
            requestId);

        // Obtener configuración de n8n desde la base de datos
        var n8nConfig = await GetN8nConfigAsync(tenantId, cancellationToken);

        // Si está en modo mock, simular
        if (n8nConfig.UseMock)
        {
            await Task.Delay(50, cancellationToken);
            return new AutomationExecutionStatus
            {
                RequestId = requestId,
                Status = "Completed"
            };
        }

        try
        {
            // n8n no tiene API directa para consultar estado, pero podemos usar la API de ejecuciones
            var n8nApiUrl = n8nConfig.ApiUrl;
            var n8nApiKey = n8nConfig.EncryptedApiKey; // Nota: Está encriptada, necesitaría desencriptar

            if (string.IsNullOrWhiteSpace(n8nApiUrl) || string.IsNullOrWhiteSpace(n8nApiKey))
            {
                _logger.LogWarning("N8n API not configured. Using mock status.");
                return new AutomationExecutionStatus
                {
                    RequestId = requestId,
                    Status = "Completed"
                };
            }

            // Consultar ejecución en n8n (requiere API de n8n)
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-N8N-API-KEY", n8nApiKey);

            var response = await _httpClient.GetAsync(
                $"{n8nApiUrl}/api/v1/executions/{requestId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Could not get execution status from n8n. StatusCode={StatusCode}",
                    response.StatusCode);
                return new AutomationExecutionStatus
                {
                    RequestId = requestId,
                    Status = "Unknown"
                };
            }

            var executionData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(
                cancellationToken);

            return new AutomationExecutionStatus
            {
                RequestId = requestId,
                Status = executionData?.ContainsKey("status") == true
                    ? executionData["status"]?.ToString() ?? "Unknown"
                    : "Unknown",
                Progress = executionData?.ContainsKey("progress") == true
                    ? Convert.ToInt32(executionData["progress"])
                    : null,
                CurrentStep = executionData?.ContainsKey("currentStep") == true
                    ? executionData["currentStep"]?.ToString()
                    : null,
                StartedAt = executionData?.ContainsKey("startedAt") == true
                    ? DateTime.Parse(executionData["startedAt"]?.ToString() ?? "")
                    : null,
                CompletedAt = executionData?.ContainsKey("completedAt") == true
                    ? DateTime.Parse(executionData["completedAt"]?.ToString() ?? "")
                    : null,
                ErrorMessage = executionData?.ContainsKey("error") == true
                    ? executionData["error"]?.ToString()
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting execution status from n8n: RequestId={RequestId}",
                requestId);
            return new AutomationExecutionStatus
            {
                RequestId = requestId,
                Status = "Error",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> CancelExecutionAsync(
        Guid tenantId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Canceling execution: TenantId={TenantId}, RequestId={RequestId}",
            tenantId,
            requestId);

        // Obtener configuración de n8n desde la base de datos
        var n8nConfig = await GetN8nConfigAsync(tenantId, cancellationToken);

        // Si está en modo mock, simular
        if (n8nConfig.UseMock)
        {
            await Task.Delay(50, cancellationToken);
            return true;
        }

        try
        {
            var n8nApiUrl = n8nConfig.ApiUrl;
            var n8nApiKey = n8nConfig.EncryptedApiKey; // Nota: Está encriptada, necesitaría desencriptar

            if (string.IsNullOrWhiteSpace(n8nApiUrl) || string.IsNullOrWhiteSpace(n8nApiKey))
            {
                _logger.LogWarning("N8n API not configured. Cannot cancel execution.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-N8N-API-KEY", n8nApiKey);

            var response = await _httpClient.PostAsync(
                $"{n8nApiUrl}/api/v1/executions/{requestId}/stop",
                null,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error canceling execution in n8n: RequestId={RequestId}",
                requestId);
            return false;
        }
    }

    public async Task ProcessWebhookResponseAsync(
        Guid tenantId,
        string requestId,
        WebhookResponseData responseData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing webhook response: TenantId={TenantId}, RequestId={RequestId}, Status={Status}",
            tenantId,
            requestId,
            responseData.Status);

        // Procesar la respuesta recibida de n8n
        // Aquí puedes actualizar el estado en la base de datos, notificar al usuario, etc.
        
        try
        {
            // TODO: Implementar lógica de procesamiento según el tipo de respuesta
            // Por ejemplo:
            // - Si es un MarketingPack generado, guardarlo
            // - Si es una publicación completada, actualizar el estado
            // - Si hay errores, registrar y notificar

            _logger.LogInformation(
                "Webhook response processed: RequestId={RequestId}, Status={Status}",
                requestId,
                responseData.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing webhook response: RequestId={RequestId}",
                requestId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la configuración de n8n desde la base de datos.
    /// </summary>
    private async Task<TenantN8nConfig> GetN8nConfigAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var configs = await _configRepository.FindAsync(
                c => c.TenantId == tenantId && c.IsActive,
                tenantId,
                cancellationToken);

            var config = configs.FirstOrDefault();

            if (config != null)
            {
                _logger.LogInformation(
                    "Configuración de n8n cargada desde BD para Tenant {TenantId}: BaseUrl={BaseUrl}, DefaultWebhookUrl={DefaultWebhookUrl}, WebhookUrlsJson={WebhookUrlsJson}",
                    tenantId,
                    config.BaseUrl,
                    config.DefaultWebhookUrl,
                    config.WebhookUrlsJson);
                
                // Actualizar contador de uso
                config.LastUsedAt = DateTime.UtcNow;
                config.UsageCount++;
                await _configRepository.UpdateAsync(config, cancellationToken);
                return config;
            }

            // Si no hay configuración en BD, usar valores por defecto de appsettings.json
            _logger.LogWarning(
                "No se encontró configuración de n8n en BD para Tenant {TenantId}, usando valores por defecto",
                tenantId);

            return new TenantN8nConfig
            {
                TenantId = tenantId,
                UseMock = bool.TryParse(_configuration["N8n:UseMock"], out var useMock) ? useMock : true,
                BaseUrl = _configuration["N8n:BaseUrl"] ?? "http://localhost:5678",
                ApiUrl = _configuration["N8n:ApiUrl"] ?? "http://localhost:5678/api/v1",
                DefaultWebhookUrl = _configuration["N8n:DefaultWebhookUrl"] ?? "http://localhost:5678/webhook",
                WebhookUrlsJson = "{}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de n8n para Tenant {TenantId}", tenantId);
            // Retornar configuración por defecto en caso de error
            return new TenantN8nConfig
            {
                TenantId = tenantId,
                UseMock = true,
                BaseUrl = "http://localhost:5678",
                ApiUrl = "http://localhost:5678/api/v1",
                DefaultWebhookUrl = "http://localhost:5678/webhook",
                WebhookUrlsJson = "{}"
            };
        }
    }

    /// <summary>
    /// Obtiene la URL del webhook según el tipo de evento desde la configuración.
    /// </summary>
    private string GetWebhookUrlForEventType(string eventType, TenantN8nConfig config)
    {
        // Deserializar webhook URLs desde JSON
        Dictionary<string, string> webhookUrls = new();
        if (!string.IsNullOrWhiteSpace(config.WebhookUrlsJson))
        {
            try
            {
                webhookUrls = JsonSerializer.Deserialize<Dictionary<string, string>>(config.WebhookUrlsJson) 
                    ?? new Dictionary<string, string>();
                _logger.LogInformation(
                    "WebhookUrls deserializados para Tenant {TenantId}: {WebhookUrlsJson}, Keys={Keys}",
                    config.TenantId,
                    config.WebhookUrlsJson,
                    string.Join(", ", webhookUrls.Keys));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar WebhookUrlsJson para Tenant {TenantId}: {WebhookUrlsJson}", 
                    config.TenantId, config.WebhookUrlsJson);
            }
        }
        else
        {
            _logger.LogWarning(
                "WebhookUrlsJson está vacío para Tenant {TenantId}, usando DefaultWebhookUrl: {DefaultWebhookUrl}",
                config.TenantId,
                config.DefaultWebhookUrl);
        }

        // Mapear tipos de eventos a URLs de webhooks de n8n
        var url = eventType.ToLower() switch
        {
            "marketing.request" => webhookUrls.GetValueOrDefault("MarketingRequest") ?? config.DefaultWebhookUrl,
            "validate.consents" => webhookUrls.GetValueOrDefault("ValidateConsents") ?? config.DefaultWebhookUrl,
            "load.memory" => webhookUrls.GetValueOrDefault("LoadMemory") ?? config.DefaultWebhookUrl,
            "analyze.instruction" => webhookUrls.GetValueOrDefault("AnalyzeInstruction") ?? config.DefaultWebhookUrl,
            "generate.strategy" => webhookUrls.GetValueOrDefault("GenerateStrategy") ?? config.DefaultWebhookUrl,
            "generate.copy" => webhookUrls.GetValueOrDefault("GenerateCopy") ?? config.DefaultWebhookUrl,
            "generate.visual.prompts" => webhookUrls.GetValueOrDefault("GenerateVisualPrompts") ?? config.DefaultWebhookUrl,
            "build.marketing.pack" => webhookUrls.GetValueOrDefault("BuildMarketingPack") ?? config.DefaultWebhookUrl,
            "human.approval" => webhookUrls.GetValueOrDefault("HumanApproval") ?? config.DefaultWebhookUrl,
            "publish.content" => webhookUrls.GetValueOrDefault("PublishContent") ?? config.DefaultWebhookUrl,
            "metrics.learning" => webhookUrls.GetValueOrDefault("MetricsLearning") ?? config.DefaultWebhookUrl,
            _ => config.DefaultWebhookUrl
        };
        
        _logger.LogInformation(
            "URL del webhook para evento {EventType}: {WebhookUrl} (desde {Source})",
            eventType,
            url,
            webhookUrls.ContainsKey("MarketingRequest") ? "WebhookUrlsJson" : "DefaultWebhookUrl");
        
        return url;
    }
}


