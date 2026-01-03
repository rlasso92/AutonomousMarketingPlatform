using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;

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
                    
                    // Obtener channels - NUNCA debe ser null o vacío (ya validado en el controller)
                    var channelsValue = eventDataDict.GetValueOrDefault("channels") ?? 
                                       eventDataDict.GetValueOrDefault("Channels");
                    
                    // Validar que channels sea una lista no vacía
                    List<string> channelsList;
                    if (channelsValue is List<string> channels)
                    {
                        channelsList = channels;
                    }
                    else if (channelsValue is IEnumerable<string> enumerable)
                    {
                        channelsList = enumerable.ToList();
                    }
                    else
                    {
                        _logger.LogWarning("channels no es una lista válida, usando lista vacía (esto causará error en n8n)");
                        channelsList = new List<string>();
                    }
                    
                    // Validar que channels no esté vacío (debería estar validado antes, pero por seguridad)
                    if (channelsList.Count == 0)
                    {
                        _logger.LogError("channels está vacío - esto causará error de validación en n8n");
                        throw new ArgumentException("channels no puede estar vacío. Debe contener al menos un canal.");
                    }
                    
                    // Usar objeto anónimo para garantizar serialización correcta de tipos
                    // Esto asegura que List<string> se serialice como array JSON correctamente
                    // IMPORTANTE: El workflow n8n espera los datos dentro de un objeto "body"
                    payload = new
                    {
                        body = new
                        {
                            tenantId = tenantId.ToString(),
                            userId = userId.Value.ToString(),
                            instruction = eventDataDict.GetValueOrDefault("instruction")?.ToString() ?? 
                                         eventDataDict.GetValueOrDefault("Instruction")?.ToString() ?? "",
                            channels = channelsList, // ✅ List<string> se serializa como array JSON ["item1", "item2"]
                            requiresApproval = eventDataDict.GetValueOrDefault("requiresApproval") ?? 
                                             eventDataDict.GetValueOrDefault("RequiresApproval") ?? false,
                            campaignId = relatedEntityId?.ToString(), // Puede ser null, se omitirá si es null
                            assets = eventDataDict.GetValueOrDefault("assets") ?? 
                                    eventDataDict.GetValueOrDefault("Assets") ?? new List<string>()
                        }
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
                    
                    // Obtener channels - NUNCA debe ser null o vacío
                    var channelsValueObj = eventDataObj?.GetValueOrDefault("channels") ?? 
                                           eventDataObj?.GetValueOrDefault("Channels");
                    
                    // Validar que channels sea una lista no vacía
                    List<string> channelsListObj;
                    if (channelsValueObj is List<string> channelsObj)
                    {
                        channelsListObj = channelsObj;
                    }
                    else if (channelsValueObj is IEnumerable<string> enumerableObj)
                    {
                        channelsListObj = enumerableObj.ToList();
                    }
                    else if (channelsValueObj is JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        // Si viene deserializado como JsonElement, convertirlo
                        channelsListObj = jsonElement.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList();
                    }
                    else
                    {
                        _logger.LogWarning("channels no es una lista válida en eventData no-Dictionary, usando lista vacía (esto causará error en n8n)");
                        channelsListObj = new List<string>();
                    }
                    
                    // Validar que channels no esté vacío
                    if (channelsListObj.Count == 0)
                    {
                        _logger.LogError("channels está vacío en eventData no-Dictionary - esto causará error de validación en n8n");
                        throw new ArgumentException("channels no puede estar vacío. Debe contener al menos un canal.");
                    }
                    
                    // Usar objeto anónimo para garantizar serialización correcta de tipos
                    // IMPORTANTE: El workflow n8n espera los datos dentro de un objeto "body"
                    payload = new
                    {
                        body = new
                        {
                            tenantId = tenantId.ToString(),
                            userId = userId.Value.ToString(),
                            instruction = eventDataObj?.GetValueOrDefault("instruction")?.ToString() ?? 
                                         eventDataObj?.GetValueOrDefault("Instruction")?.ToString() ?? "",
                            channels = channelsListObj, // ✅ List<string> se serializa como array JSON
                            requiresApproval = eventDataObj?.GetValueOrDefault("requiresApproval") ?? 
                                             eventDataObj?.GetValueOrDefault("RequiresApproval") ?? false,
                            campaignId = relatedEntityId?.ToString(), // Puede ser null
                            assets = eventDataObj?.GetValueOrDefault("assets") ?? 
                                    eventDataObj?.GetValueOrDefault("Assets") ?? new List<string>()
                        }
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
            
            // Serializar el payload con opciones que garanticen tipos correctos
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            // Serializar para logging y envío
            var jsonContent = JsonSerializer.Serialize(payload, jsonOptions);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            // Log detallado del payload completo
            _logger.LogInformation("=== PAYLOAD ENVIADO A N8N ===");
            _logger.LogInformation("URL: {WebhookUrl}", webhookUrl);
            _logger.LogInformation("Método: POST");
            _logger.LogInformation("Content-Type: application/json");
            _logger.LogInformation("Payload JSON (compacto): {Payload}", jsonContent);
            
            // Log con formato legible para debugging
            var jsonFormatted = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            _logger.LogInformation("Payload JSON (formateado):\n{FormattedPayload}", jsonFormatted);
            
            // Log de tipos del payload (usando reflexión para objetos anónimos)
            _logger.LogInformation("--- Tipos del Payload ---");
            _logger.LogInformation("  Tipo del payload: {PayloadType}", payload.GetType().Name);
            if (payload is Dictionary<string, object> dictPayload)
            {
                foreach (var kvp in dictPayload)
                {
                    var valueStr = kvp.Value switch
                    {
                        List<string> list => $"[{string.Join(", ", list)}]",
                        null => "null",
                        _ => kvp.Value.ToString() ?? "null"
                    };
                    _logger.LogInformation("  {Key}: {Value} (Tipo: {Type})", 
                        kvp.Key, 
                        valueStr, 
                        kvp.Value?.GetType().Name ?? "null");
                }
            }
            else
            {
                // Para objetos anónimos, mostrar el JSON formateado
                _logger.LogInformation("  Payload es objeto anónimo - ver JSON formateado arriba");
            }
            _logger.LogInformation("=== FIN PAYLOAD ===");
            
            // Enviar el request
            response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
            
            _logger.LogInformation(
                "Respuesta de n8n: StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}",
                response.StatusCode,
                response.ReasonPhrase);

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
                BaseUrl = _configuration["N8n:BaseUrl"] ?? "https://n8n.bashpty.com",
                ApiUrl = _configuration["N8n:ApiUrl"] ?? "https://n8n.bashpty.com/api/v1",
                DefaultWebhookUrl = _configuration["N8n:DefaultWebhookUrl"] ?? "https://n8n.bashpty.com/webhook/marketing-request",
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
        // Si hay una URL específica en WebhookUrlsJson, usarla
        // Si no, construir la URL usando BaseUrl + path del evento
        var url = eventType.ToLower() switch
        {
            "marketing.request" => webhookUrls.GetValueOrDefault("MarketingRequest") 
                ?? BuildWebhookUrl(config.BaseUrl, "marketing-request", config.DefaultWebhookUrl),
            "validate.consents" => webhookUrls.GetValueOrDefault("ValidateConsents") 
                ?? BuildWebhookUrl(config.BaseUrl, "validate-consents", config.DefaultWebhookUrl),
            "load.memory" => webhookUrls.GetValueOrDefault("LoadMemory") 
                ?? BuildWebhookUrl(config.BaseUrl, "load-memory", config.DefaultWebhookUrl),
            "analyze.instruction" => webhookUrls.GetValueOrDefault("AnalyzeInstruction") 
                ?? BuildWebhookUrl(config.BaseUrl, "analyze-instruction", config.DefaultWebhookUrl),
            "generate.strategy" => webhookUrls.GetValueOrDefault("GenerateStrategy") 
                ?? BuildWebhookUrl(config.BaseUrl, "generate-strategy", config.DefaultWebhookUrl),
            "generate.copy" => webhookUrls.GetValueOrDefault("GenerateCopy") 
                ?? BuildWebhookUrl(config.BaseUrl, "generate-copy", config.DefaultWebhookUrl),
            "generate.visual.prompts" => webhookUrls.GetValueOrDefault("GenerateVisualPrompts") 
                ?? BuildWebhookUrl(config.BaseUrl, "generate-visual-prompts", config.DefaultWebhookUrl),
            "build.marketing.pack" => webhookUrls.GetValueOrDefault("BuildMarketingPack") 
                ?? BuildWebhookUrl(config.BaseUrl, "build-marketing-pack", config.DefaultWebhookUrl),
            "human.approval" => webhookUrls.GetValueOrDefault("HumanApproval") 
                ?? BuildWebhookUrl(config.BaseUrl, "human-approval", config.DefaultWebhookUrl),
            "publish.content" => webhookUrls.GetValueOrDefault("PublishContent") 
                ?? BuildWebhookUrl(config.BaseUrl, "publish-content", config.DefaultWebhookUrl),
            "metrics.learning" => webhookUrls.GetValueOrDefault("MetricsLearning") 
                ?? BuildWebhookUrl(config.BaseUrl, "metrics-learning", config.DefaultWebhookUrl),
            _ => config.DefaultWebhookUrl
        };
        
        // Determinar la fuente para el log (mejorado para distinguir BuildWebhookUrl vs DefaultWebhookUrl)
        string source;
        if (eventType.ToLower() == "marketing.request" && webhookUrls.ContainsKey("MarketingRequest"))
        {
            source = "WebhookUrlsJson";
        }
        else if (eventType.ToLower() == "marketing.request" && url != config.DefaultWebhookUrl && url.Contains("/webhook/marketing-request"))
        {
            source = "BuildWebhookUrl(BaseUrl)";
        }
        else
        {
            source = "DefaultWebhookUrl";
        }
        
        _logger.LogInformation(
            "URL del webhook para evento {EventType}: {WebhookUrl} (desde {Source}, BaseUrl={BaseUrl})",
            eventType,
            url,
            source,
            config.BaseUrl);
        
        return url;
    }

    /// <summary>
    /// Construye la URL del webhook usando BaseUrl + path del evento.
    /// Si BaseUrl no está configurado, usa el DefaultWebhookUrl como fallback.
    /// </summary>
    private string BuildWebhookUrl(string? baseUrl, string eventPath, string defaultWebhookUrl)
    {
        // Si BaseUrl está configurado y no es localhost (producción), construir URL completa
        if (!string.IsNullOrWhiteSpace(baseUrl) && 
            (baseUrl.Contains("n8n.bashpty.com") || baseUrl.Contains("https://") || baseUrl.Contains("http://")))
        {
            // Asegurar que BaseUrl no termine con /
            var cleanBaseUrl = baseUrl.TrimEnd('/');
            // Construir URL: https://n8n.bashpty.com/webhook/marketing-request
            return $"{cleanBaseUrl}/webhook/{eventPath}";
        }
        
        // Si no hay BaseUrl válido, usar DefaultWebhookUrl
        return defaultWebhookUrl;
    }
}


